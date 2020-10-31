using System;
using UnityEngine;

public static class KernelConfigController
{
    static KernelConfig config = null;

    static public KernelConfig GetConfig()
    {
        if (!config)
        {
            config = Resources.Load<KernelConfig>("KernelConfiguration");
        }
        return config;
    }


    static internal void SetConfig(KernelConfigModel newConfig)
    {
        GetConfig().Set(newConfig);
    }

    static internal void SetConfig(string configJson)
    {
        try
        {
            var prevConfig = GetConfig().Get().Clone();
            JsonUtility.FromJsonOverwrite(configJson, prevConfig);
            SetConfig(prevConfig);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting KernelConfig {e.Message}");
        }
    }
}
