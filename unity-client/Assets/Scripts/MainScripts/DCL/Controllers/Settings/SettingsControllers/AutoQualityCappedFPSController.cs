using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.SettingsData;
using UnityEngine;

public class AutoQualityCappedFPSController : IAutoQualityController
{
    private const int EVALUATIONS_SIZE = 5;
    private const float INCREASE_MARGIN = 0.9f;
    private const float STAY_MARGIN = 0.8f;

    internal int targetFPS;
    internal int currentQualityIndex;
    internal readonly QualitySettingsData qualitySettings;

    private readonly List<float> fpsEvaluations = new List<float>();

    public AutoQualityCappedFPSController(int targetFPS, int startIndex, QualitySettingsData qualitySettings)
    {
        this.targetFPS = targetFPS;
        currentQualityIndex = startIndex;
        this.qualitySettings = qualitySettings;
    }

    public int EvaluateQuality(PerformanceMetricsData metrics)
    {
        if (metrics == null) return 0;

        fpsEvaluations.Add(metrics.fpsCount);
        if (fpsEvaluations.Count <= EVALUATIONS_SIZE)
            return 0;

        fpsEvaluations.RemoveAt(0);
        float performance = fpsEvaluations.Average() / targetFPS;

        int newCurrentQualityIndex = currentQualityIndex;
        if (performance < STAY_MARGIN)
            newCurrentQualityIndex = Mathf.Max(0, currentQualityIndex - 1);

        if (performance >= INCREASE_MARGIN)
            newCurrentQualityIndex = Mathf.Min(qualitySettings.Length - 1, currentQualityIndex + 2); //We increase quality more aggressively than we reduce

        if (newCurrentQualityIndex != currentQualityIndex)
            ResetEvaluation();

        currentQualityIndex = newCurrentQualityIndex;
        return currentQualityIndex;
    }

    public void ResetEvaluation()
    {
        fpsEvaluations.Clear();
    }
}