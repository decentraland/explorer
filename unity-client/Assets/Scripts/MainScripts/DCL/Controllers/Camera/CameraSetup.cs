using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("CameraTests")]
public abstract class CameraSetup
{
    public abstract void Activate();
    public abstract void Deactivate();

    public abstract void Update(float deltaTimeInSecs);
}

public abstract class CameraSetup<TConfig> : CameraSetup, IDisposable
{
    internal Camera camera;
    internal BaseVariable<TConfig> configuration;

    public CameraSetup( Camera camera, BaseVariable<TConfig> configuration)
    {
        this.camera = camera;
        this.configuration = configuration;
    }

    public sealed override void Activate()
    {
        configuration.OnChange += OnConfigChanged;
        SetUp();
    }

    public sealed  override void Deactivate()
    {
        configuration.OnChange -= OnConfigChanged;
    }

    protected abstract void SetUp();
    protected abstract void OnConfigChanged(TConfig newConfig, TConfig oldConfig);

    public virtual void Dispose()
    {
        configuration.OnChange -= OnConfigChanged;
        CleanUp();
    }

    protected abstract void CleanUp();
}