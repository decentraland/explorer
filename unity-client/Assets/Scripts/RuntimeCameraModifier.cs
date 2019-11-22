using System;
using UnityEngine;
using UnityEngine.UI;

public class RuntimeCameraModifier : MonoBehaviour
{
    public ThirdPersonCameraConfigSO config;
    public CameraStateSO cameraState;
    public Canvas canvas;

    public float heightMin;
    public float heightMax;
    public Slider heightSlider;
    public Text heightText;

    public float depthMin;
    public float depthMax;
    public Slider depthSlider;
    public Text depthText;

    private void Start()
    {
        canvas.enabled = cameraState == CameraController.CameraState.ThirdPerson;
        cameraState.OnChange += ( current,  previous) => canvas.enabled = current == CameraController.CameraState.ThirdPerson;

        heightSlider.onValueChanged.AddListener(HeightChanged);
        heightSlider.value = Mathf.InverseLerp(heightMin, heightMax, config.Get().offset.y);

        depthSlider.onValueChanged.AddListener(DepthChanged);
        depthSlider.value = Mathf.InverseLerp(depthMin, depthMax, config.Get().offset.z);
    }

    private void DepthChanged(float value)
    {
        var realValue = Mathf.Lerp(depthMin, depthMax, value);
        depthText.text = realValue.ToString();
        config.Set(new ThirdPersonCameraConfig()
        {
            offset = Vector3.Scale(config.Get().offset, new Vector3(1, 1, 0)) + (Vector3.forward * realValue)
        });
    }

    private void HeightChanged(float value)
    {
        var realValue = Mathf.Lerp(heightMin, heightMax, value);
        heightText.text = realValue.ToString();
        config.Set(new ThirdPersonCameraConfig()
        {
            offset = Vector3.Scale(config.Get().offset, new Vector3(1, 0, 1)) + (Vector3.up * realValue)
        });
    }
}