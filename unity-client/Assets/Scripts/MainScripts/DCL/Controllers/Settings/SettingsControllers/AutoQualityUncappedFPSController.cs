using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.FPSDisplay;
using DCL.SettingsData;
using UnityEngine;

public class AutoQualityUncappedFPSController : IAutoQualityController
{
    private const int EVALUATIONS_SIZE = 5;

    internal int currentQualityIndex;
    internal readonly QualitySettingsData qualitySettings;

    private readonly List<float> fpsEvaluations = new List<float>();

    public AutoQualityUncappedFPSController(int startIndex, QualitySettingsData qualitySettings)
    {
        currentQualityIndex = startIndex;
        this.qualitySettings = qualitySettings;
    }

    public int EvaluateQuality(PerformanceMetricsData metrics)
    {
        if (metrics == null) return 0;

        //TODO refine this evaluation
        fpsEvaluations.Add(metrics.fpsCount);
        if (fpsEvaluations.Count <= EVALUATIONS_SIZE)
            return 0;

        fpsEvaluations.RemoveAt(0);
        float average = fpsEvaluations.Average();

        int newCurrentQualityIndex = currentQualityIndex;
        if (average <= FPSEvaluation.WORSE)
            newCurrentQualityIndex = Mathf.Max(0, currentQualityIndex - 1);

        if (average >= FPSEvaluation.GREAT)
            newCurrentQualityIndex = Mathf.Min(qualitySettings.Length - 1, currentQualityIndex + 1);

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