using DCL;

public interface IAutoQualityController
{
    int EvaluateQuality(PerformanceMetricsData metrics);
    void ResetEvaluation();
}