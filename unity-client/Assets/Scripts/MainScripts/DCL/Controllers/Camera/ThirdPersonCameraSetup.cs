using System;
using UnityEngine;

public class ThirdPersonCameraSetup : CameraSetup<ThirdPersonCameraConfig>
{
    private Vector3 initialLocalPosition;
    private Vector3 desiredLocalPosition;

    private float initialFoV;
    private float desiredFoV;

    private float currentTime = 0;

    public ThirdPersonCameraSetup(Camera camera, BaseVariable<ThirdPersonCameraConfig> configuration) : base(camera, configuration) { }

    protected override void SetUp()
    {
        currentTime = 0;

        initialLocalPosition = camera.transform.localPosition;
        desiredLocalPosition = configuration.Get().offset;

        initialFoV = camera.fieldOfView;
        desiredFoV = configuration.Get().fieldOfView;
    }

    protected override void OnConfigChanged(ThirdPersonCameraConfig newConfig, ThirdPersonCameraConfig oldConfig)
    {
        SetUp();
    }

    protected override void CleanUp() { }

    public override void Update(float deltaTimeInSecs)
    {
        var newTime = Mathf.Clamp(currentTime + deltaTimeInSecs, 0, configuration.Get().transitionTime);
        if (Math.Abs(currentTime - newTime) < Mathf.Epsilon) return;

        currentTime = newTime;
        camera.transform.localPosition = Vector3.Lerp(initialLocalPosition, desiredLocalPosition, currentTime / configuration.Get().transitionTime);
        camera.fieldOfView = Mathf.Lerp(initialFoV, desiredFoV, currentTime / configuration.Get().transitionTime);
    }
}