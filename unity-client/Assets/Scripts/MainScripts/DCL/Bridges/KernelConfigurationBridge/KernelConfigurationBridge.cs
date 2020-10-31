using UnityEngine;

public class KernelConfigurationBridge : MonoBehaviour
{
    public void SetKernelConfiguration(string json)
    {
        KernelConfigController.SetConfig(json);
    }
}
