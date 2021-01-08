using Cinemachine;
using DCL.SettingsController;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mouse Sensivity", fileName = "MouseSensivityControlController")]
    public class MouseSensivityControlController : SettingsControlController
    {
        const string MOUSE_SENSIVITY_SETTINGS_KEY = "Settings.MouseSensivity";
        const float FIRST_PERSON_MIN_SPEED = 25f;
        const float FIRST_PERSON_MAX_SPEED = 350f;
        const float THIRD_PERSON_X_MIN_SPEED = 100f;
        const float THIRD_PERSON_X_MAX_SPEED = 450f;
        const float THIRD_PERSON_Y_MIN_SPEED = 0.5f;
        const float THIRD_PERSON_Y_MAX_SPEED = 3f;

        private GeneralSettingsController generalSettings; // TODO (Santi): Refactorize!
        private Slider mouseSensitivitySlider;
        private CinemachinePOV povCamera;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            generalSettings = GameObject.FindObjectOfType<GeneralSettingsController>();
            povCamera = generalSettings.firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
            mouseSensitivitySlider = ((SliderSettingsControlView)view).sliderControl;
        }

        public override object GetStoredValue()
        {
            float storedValue = PlayerPrefs.GetFloat(MOUSE_SENSIVITY_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return MouseSensivityToSliderValue(storedValue);
            else
                return MouseSensivityToSliderValue(Settings.i.GetDefaultGeneralSettings().mouseSensitivity);
        }

        private float MouseSensivityToSliderValue(float mouseSensivityValue)
        {
            return Mathf.Lerp(mouseSensitivitySlider.minValue, mouseSensitivitySlider.maxValue, mouseSensivityValue);
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            float newMouseSensivityValue = RemapMouseSensitivityTo01((float)newValue);

            var povSpeed = Mathf.Lerp(FIRST_PERSON_MIN_SPEED, FIRST_PERSON_MAX_SPEED, newMouseSensivityValue);
            povCamera.m_HorizontalAxis.m_MaxSpeed = povSpeed;
            povCamera.m_VerticalAxis.m_MaxSpeed = povSpeed;
            generalSettings.thirdPersonCamera.m_XAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_X_MIN_SPEED, THIRD_PERSON_X_MAX_SPEED, newMouseSensivityValue);
            generalSettings.thirdPersonCamera.m_YAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_Y_MIN_SPEED, THIRD_PERSON_Y_MAX_SPEED, newMouseSensivityValue);

            PlayerPrefs.SetFloat(MOUSE_SENSIVITY_SETTINGS_KEY, newMouseSensivityValue);
        }

        private float RemapMouseSensitivityTo01(float value)
        {
            return (value - mouseSensitivitySlider.minValue)
                / (mouseSensitivitySlider.maxValue - mouseSensitivitySlider.minValue)
                * (1 - 0) + 0; //(value - from1) / (to1 - from1) * (to2 - from2) + from2
        }
    }
}