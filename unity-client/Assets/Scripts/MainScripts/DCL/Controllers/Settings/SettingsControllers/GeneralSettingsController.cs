using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsController
{
    public class GeneralSettingsController : MonoBehaviour
    {
        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        private CinemachinePOV povCamera;

        void Awake()
        {
            povCamera = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        void Start()
        {
            ApplyGeneralSettings(Settings.i.generalSettings);
        }

        void OnEnable()
        {
            Settings.i.OnGeneralSettingsChanged += ApplyGeneralSettings;
        }

        void OnDisable()
        {
            Settings.i.OnGeneralSettingsChanged -= ApplyGeneralSettings;
        }

        void ApplyGeneralSettings(DCL.SettingsData.GeneralSettings settings)
        {
            // float thirdpersonSensitivity = (1f - settings.mouseSensitivity).Remap(0, 1, THIRDPERSON_MIN_MOUSE_SENSITIVITY, THIRDPERSON_MAX_MOUSE_SENSITIVITY);
            // float firstPersonSpeed = Mathf.Lerp(FIRSTPERSON_MIN_SPEED, FIRSTPERSON_MAX_SPEED, settings.mouseSensitivity);
            //
            // thirdPersonCamera.m_XAxis.m_AccelTime = firstPersonSpeed;
            // thirdPersonCamera.m_YAxis.m_AccelTime = firstPersonSpeed;
            //
            // povCamera.m_HorizontalAxis.m_MaxSpeed = firstPersonSpeed;
            // povCamera.m_VerticalAxis.m_MaxSpeed = firstPersonSpeed;
            // povCamera.m_HorizontalAxis.m_AccelTime = 0;
            // povCamera.m_VerticalAxis.m_AccelTime = 0;

            AudioListener.volume = settings.sfxVolume;
            DCL.Interface.WebInterface.ApplySettings(settings.voiceChatVolume, (int)settings.voiceChatAllow);
        }


        [SerializeField] private Slider firstSpeed, firstAccel;
        [SerializeField] private Slider thirdXSpeed, thirdXAccel;
        [SerializeField] private Slider thirdYSpeed, thirdYAccel;

        [SerializeField] private TextMeshProUGUI firstSpeedText, firstAccelText;
        [SerializeField] private TextMeshProUGUI thirdXSpeedText, thirdXAccelText;
        [SerializeField] private TextMeshProUGUI thirdYSpeedText, thirdYAccelText;

        private void Update()
        {
            firstSpeedText.text = firstSpeed.value.ToString();
            firstAccelText.text = firstAccel.value.ToString();

            thirdXSpeedText.text = thirdXSpeed.value.ToString();
            thirdXAccelText.text = thirdXAccel.value.ToString();
            thirdYSpeedText.text = thirdYSpeed.value.ToString();
            thirdYAccelText.text = thirdYAccel.value.ToString();


            povCamera.m_HorizontalAxis.m_MaxSpeed  = firstSpeed.value;
            povCamera.m_VerticalAxis.m_MaxSpeed  = firstSpeed.value;
            povCamera.m_HorizontalAxis.m_AccelTime = firstAccel.value;
            povCamera.m_VerticalAxis.m_AccelTime = firstAccel.value;

            thirdPersonCamera.m_XAxis.m_MaxSpeed = thirdXSpeed.value;
            thirdPersonCamera.m_YAxis.m_MaxSpeed = thirdYSpeed.value;
            thirdPersonCamera.m_XAxis.m_AccelTime = thirdXAccel.value;
            thirdPersonCamera.m_YAxis.m_AccelTime = thirdYAccel.value;


        }
    }
}