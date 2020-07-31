using DCL;
using Kongregate;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PoolsCounterDisplay : MonoBehaviour
{
    public Text label;

    private void Update()
    {
        if (label == null)
            return;

        string targetText = string.Empty;

#if UNITY_WEBGL && !UNITY_EDITOR
        targetText += $"Free Memory: {(WebGLMemoryStats.GetFreeMemorySize() * 100f / WebGLMemoryStats.GetTotalMemorySize()).ToString("##.00")}%\n";
#else
        targetText += "Free Memory: 0.00%\n";
#endif

        targetText += $"Total Pools: {PoolManager.i.pools.Count} (Not Persistent: {PoolManager.i.pools.Count(p => !p.Value.persistent)})\n";

        if (label.text != targetText)
        {
            label.text = targetText;
        }
    }
}
