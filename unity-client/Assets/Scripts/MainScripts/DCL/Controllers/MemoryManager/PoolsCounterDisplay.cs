using DCL;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL && !UNITY_EDITOR
using Kongregate;
#endif

public class PoolsCounterDisplay : MonoBehaviour
{
    public Text label;

    private void Update()
    {
        if (label == null)
            return;

        string targetText = string.Empty;

#if UNITY_WEBGL && !UNITY_EDITOR
        targetText += $"Used Memory: {WebGLMemoryStats.GetUsedMemorySize() / 1024 / 1024}MB/{WebGLMemoryStats.GetTotalMemorySize() / 1024 / 1024}MB ({(WebGLMemoryStats.GetFreeMemorySize() * 100f / WebGLMemoryStats.GetTotalMemorySize()).ToString("##.00")}%)\n";
#endif

        targetText += $"Total Pools: {PoolManager.i.pools.Count} (Not Persistent: {PoolManager.i.pools.Count(p => !p.Value.persistent)})\n";

        if (label.text != targetText)
        {
            label.text = targetText;
        }
    }
}
