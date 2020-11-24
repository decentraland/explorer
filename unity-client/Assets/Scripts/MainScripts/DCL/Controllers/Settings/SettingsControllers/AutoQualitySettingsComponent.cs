using System.Collections;
using DCL;
using DCL.FPSDisplay;
using DCL.SettingsData;
using UnityEngine;
using QualitySettings = DCL.SettingsData.QualitySettings;

/// <summary>
/// Controller to change the quality settings automatically based on performance
/// </summary>
public class AutoQualitySettingsComponent : MonoBehaviour
{
    private const float LOOP_TIME_SECONDS = 1f;
    private const string LAST_QUALITY_INDEX = "LAST_QUALITY_INDEX";

    [SerializeField] internal BooleanVariable autoQualityEnabled;
    [SerializeField] internal PerformanceMetricsDataVariable performanceMetricsData;
    internal QualitySettingsData qualitySettings => Settings.i.autoqualitySettings;

    internal int currentQualityIndex;
    private Coroutine settingsCheckerCoroutine;
    private AutoQualitySettingsController controller;

    void Start()
    {
        if (autoQualityEnabled == null || qualitySettings == null || qualitySettings.Length == 0)
            return;

        currentQualityIndex = PlayerPrefs.GetInt(LAST_QUALITY_INDEX,(qualitySettings.Length - 1) / 2);

        controller = new AutoQualitySettingsController(currentQualityIndex, qualitySettings);

        autoQualityEnabled.OnChange += SetAutoSettings;
        SetAutoSettings(autoQualityEnabled.Get(), !autoQualityEnabled.Get());
    }

    private void SetAutoSettings(bool newValue, bool oldValue)
    {
        if (settingsCheckerCoroutine != null)
        {
            StopCoroutine(settingsCheckerCoroutine);
            settingsCheckerCoroutine = null;
        }

        if (newValue)
        {
            settingsCheckerCoroutine = StartCoroutine(AutoSettingsLoop());
        }
        else
        {
            controller.ResetEvaluation();
        }
    }

    private IEnumerator AutoSettingsLoop()
    {
        while (true)
        {
            UpdateQuality(controller.EvaluateQuality(performanceMetricsData?.Get()));
            yield return new WaitForSeconds(LOOP_TIME_SECONDS);
        }
    }

    private void UpdateQuality(int newQualityIndex)
    {
        if (newQualityIndex == currentQualityIndex)
            return;

        if (newQualityIndex <= 0 || newQualityIndex >= qualitySettings.Length)
            return;

        PlayerPrefs.SetInt(LAST_QUALITY_INDEX, currentQualityIndex);
        currentQualityIndex = newQualityIndex;
        Settings.i.ApplyAutoQualitySettings(currentQualityIndex);
    }
}


public class AutoQualitySettingsController
{
    internal int currentQualityIndex;
    internal readonly QualitySettingsData qualitySettings;
    internal readonly IAutoQualitySettingsEvaluator evaluator;

    public AutoQualitySettingsController(int startIndex, QualitySettingsData qualitySettings)
    {
        currentQualityIndex = startIndex;
        this.qualitySettings = qualitySettings;
        evaluator = new AutoQualitySettingsEvaluator(FPSEvaluation.WORSE, FPSEvaluation.GREAT);
    }

    public int EvaluateQuality(PerformanceMetricsData metrics)
    {
        switch (evaluator.Evaluate(metrics))
        {
            case -1:
                currentQualityIndex = Mathf.Max(0, currentQualityIndex - 1);
                break;
            case 1:
                currentQualityIndex = Mathf.Min(qualitySettings.Length - 1, currentQualityIndex + 1);
                break;
            default:
                return currentQualityIndex;
        }

        ResetEvaluation();
        return currentQualityIndex;
    }

    public void ResetEvaluation()
    {
        evaluator.Reset();
    }
}