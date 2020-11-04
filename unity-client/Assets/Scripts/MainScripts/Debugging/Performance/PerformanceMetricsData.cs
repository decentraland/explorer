using System;
using UnityEngine;

namespace DCL
{
    [Serializable]
    public class PerformanceMetricsData
    {
        public float fpsCount;
        public int hiccupCount;
        public float hiccupSum;
        public float totalSeconds;
    }

    [CreateAssetMenu(fileName = "PerformanceMetricsDataVariable", menuName = "Variables/PerformanceMetricsDataVariable")]
    public class PerformanceMetricsDataVariable : BaseVariable<PerformanceMetricsData>
    {
        public override bool Equals(PerformanceMetricsData other)
        {
            return other == value;
        }

        public void Set(float fpsCount, int hiccuptCount, float hiccupSum, float totalSeconds)
        {
            value.fpsCount = fpsCount;
            value.hiccupCount = hiccuptCount;
            value.hiccupSum = hiccupSum;
            value.totalSeconds = totalSeconds;
            RaiseOnChange();
        }
    }
}