using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FirstPersonCameraConfig", menuName = "CameraConfig/FirstPerson")]
public class FirstPersonCameraConfigSO : BaseVariable<FirstPersonCameraConfig> { }

[Serializable]
public class FirstPersonCameraConfig
{
    public float yOffset;
    public float transitionTime;
    public float fieldOfView;
}