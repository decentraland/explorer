using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

[CreateAssetMenu(fileName = "KernelConfiguration", menuName = "KernelConfiguration")]
public class KernelConfig : ScriptableObject
{
    public delegate void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous);

    public event OnKernelConfigChanged OnChange;

    [SerializeField] KernelConfigModel value;

    List<Promise<KernelConfigModel>> initializationPromises = new List<Promise<KernelConfigModel>>();
    internal bool initialized = false;

    public void Set(KernelConfigModel newValue)
    {
        if (newValue == null)
        {
            return;
        }

        if (!initialized)
        {
            Initialize(newValue);
        }

        if (newValue.Equals(value))
            return;

        var previous = value;
        value = newValue;
        OnChange?.Invoke(value, previous);
    }

    public KernelConfigModel Get()
    {
        return value;
    }

    public Promise<KernelConfigModel> EnsureConfigInitialized()
    {
        var newPromise = new Promise<KernelConfigModel>();
        if (initialized)
        {
            newPromise.Resolve(value);
        }
        else
        {
            initializationPromises.Add(newPromise);
        }
        return newPromise;
    }

    private void Initialize(KernelConfigModel newValue)
    {
        if (initializationPromises?.Count > 0)
        {
            for (int i = 0; i < initializationPromises.Count; i++)
            {
                initializationPromises[i].Resolve(newValue);
            }
            initializationPromises.Clear();
            initializationPromises = null;
        }
        initialized = true;
    }
}
