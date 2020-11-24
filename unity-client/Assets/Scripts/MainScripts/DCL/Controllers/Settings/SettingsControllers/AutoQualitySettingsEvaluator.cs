using System.Collections.Generic;
using System.Linq;
using DCL;

public interface IAutoQualitySettingsEvaluator
{
    void Reset();
    int Evaluate(PerformanceMetricsData performanceMetrics);
}

/// <summary>
/// This class evaluate a performance metrics data to determine if the quality should be increased or decreased
/// </summary>
public class AutoQualitySettingsEvaluator : IAutoQualitySettingsEvaluator
{
    private const int EVALUATIONS_SIZE = 5;

    private readonly List<float> fpsEvaluations = new List<float>();

    private readonly int minAcceptableFPS;
    private readonly int maxAcceptableFPS;
    public AutoQualitySettingsEvaluator(int minAcceptableFPS, int maxAcceptableFPS)
    {
        this.minAcceptableFPS = minAcceptableFPS;
        this.maxAcceptableFPS = maxAcceptableFPS;
    }

    public void Reset()
    {
        fpsEvaluations.Clear();
    }

    /// <summary>
    /// Evaluate a performance metrics
    /// </summary>
    /// <param name="performanceMetrics"></param>
    /// <returns>-1 bad performance, 0 acceptable, 1 good performance</returns>
    public int Evaluate(PerformanceMetricsData performanceMetrics)
    {
        if (performanceMetrics == null) return 0;

        //TODO refine this evaluation
        fpsEvaluations.Add(performanceMetrics.fpsCount);
        if (fpsEvaluations.Count <= EVALUATIONS_SIZE)
            return 0;

        fpsEvaluations.RemoveAt(0);
        float average = fpsEvaluations.Average();
        if (average <= minAcceptableFPS)
            return -1;

        if (average >= maxAcceptableFPS)
            return 1;

        return 0;
    }
}