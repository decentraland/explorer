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

    public float foVMin;
    public float foVMax;
    public Slider foVSlider;
    public Text foVText;

    public Button[] presetButtons;
    public Button currentButton;
    private ThirdPersonCameraConfigSO[] presetConfigs;
    private ThirdPersonCameraConfigSO currentPresetConfig;

    private void Awake()
    {
        presetConfigs = new ThirdPersonCameraConfigSO[presetButtons.Length];
        for (var i = 0; i < presetButtons.Length; i++)
        {
            presetConfigs[i] = ScriptableObject.CreateInstance<ThirdPersonCameraConfigSO>();
            presetConfigs[i].Set(config);
            if (currentPresetConfig == null)
            {
                currentPresetConfig = presetConfigs[i];
                currentButton = presetButtons[i];
                currentButton.image.color = Color.green;
            }
            int index = i;
            presetButtons[i].onClick.AddListener(() =>
            {
                currentPresetConfig = presetConfigs[index];
                if(currentButton != null)
                    currentButton.image.color = Color.white;
                currentButton = presetButtons[index];
                currentButton.image.color = Color.green;
                UpdateSliders();
                UpdateRealConfig();
            });
        }
    }

    private void Start()
    {
        canvas.enabled = cameraState == CameraController.CameraState.ThirdPerson;
        cameraState.OnChange += ( current,  previous) => canvas.enabled = current == CameraController.CameraState.ThirdPerson;

        heightSlider.onValueChanged.AddListener(HeightChanged);
        depthSlider.onValueChanged.AddListener(DepthChanged);
        foVSlider.onValueChanged.AddListener(FoVChanged);

        UpdateSliders();
    }

    private void UpdateSliders()
    {
        foVSlider.value = Mathf.InverseLerp(foVMin, foVMax, currentPresetConfig.Get().fieldOfView);
        depthSlider.value = Mathf.InverseLerp(depthMin, depthMax, currentPresetConfig.Get().offset.z);
        heightSlider.value = Mathf.InverseLerp(heightMin, heightMax, currentPresetConfig.Get().offset.y);
    }

    private void HeightChanged(float value)
    {
        var realValue = Mathf.Lerp(heightMin, heightMax, value);
        heightText.text = realValue.ToString();
        currentPresetConfig.Set(new ThirdPersonCameraConfig()
        {
            offset = Vector3.Scale(currentPresetConfig.Get().offset, new Vector3(1, 0, 1)) + (Vector3.up * realValue),
            transitionTime = currentPresetConfig.Get().transitionTime,
            fieldOfView = currentPresetConfig.Get().fieldOfView,
        });
        UpdateRealConfig();
    }

    private void DepthChanged(float value)
    {
        var realValue = Mathf.Lerp(depthMin, depthMax, value);
        depthText.text = realValue.ToString();
        currentPresetConfig.Set(new ThirdPersonCameraConfig()
        {
            offset = Vector3.Scale(currentPresetConfig.Get().offset, new Vector3(1, 1, 0)) + (Vector3.forward * realValue),
            transitionTime = currentPresetConfig.Get().transitionTime,
            fieldOfView = currentPresetConfig.Get().fieldOfView,
        });
        UpdateRealConfig();
    }

    private void FoVChanged(float value)
    {
        var realValue = Mathf.Lerp(foVMin, foVMax, value);
        foVText.text = realValue.ToString();
        currentPresetConfig.Set(new ThirdPersonCameraConfig()
        {
            offset = currentPresetConfig.Get().offset,
            transitionTime = currentPresetConfig.Get().transitionTime,
            fieldOfView = realValue,
        });
        UpdateRealConfig();
    }

    private void UpdateRealConfig()
    {
        config.Set(currentPresetConfig);
    }
}