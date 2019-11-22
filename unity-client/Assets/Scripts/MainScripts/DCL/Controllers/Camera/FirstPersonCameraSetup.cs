using System;
using System.Runtime.CompilerServices;
using DCL.Helpers;
using UnityEngine;

[assembly: InternalsVisibleTo("CameraTests")]

public class FirstPersonCameraSetup : CameraSetup<FirstPersonCameraConfig>
{
    private Vector3 initialLocalPosition;
    private Vector3 desiredLocalPosition;

    private float initialFoV;
    private float desiredFoV;

    private float currentTime = 0;

    public FirstPersonCameraSetup(Camera camera, BaseVariable<FirstPersonCameraConfig> configuration) : base( camera, configuration) { }

    protected override void SetUp()
    {
        currentTime = 0;

        initialLocalPosition = camera.transform.localPosition;
        desiredLocalPosition = Vector3.up * configuration.Get().yOffset;

        initialFoV = camera.fieldOfView;
        desiredFoV = configuration.Get().fieldOfView;
    }

    protected override void OnConfigChanged(FirstPersonCameraConfig newConfig, FirstPersonCameraConfig oldConfig)
    {
        SetUp();
    }

    public override void Update(float deltaTimeInSecs)
    {
        var newTime = Mathf.Clamp(currentTime + deltaTimeInSecs, 0, configuration.Get().transitionTime);
        if (!(Math.Abs(currentTime - newTime) > Mathf.Epsilon)) return;

        currentTime = newTime;
        camera.transform.localPosition = Vector3.Lerp(initialLocalPosition, desiredLocalPosition, currentTime / configuration.Get().transitionTime);
        camera.fieldOfView = Mathf.Lerp(initialFoV, desiredFoV, currentTime / configuration.Get().transitionTime);
    }

    protected override void CleanUp() { }
}