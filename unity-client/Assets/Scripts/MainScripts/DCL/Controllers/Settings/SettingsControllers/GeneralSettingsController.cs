using Cinemachine;
using UnityEngine;

namespace DCL.SettingsController
{
    public class GeneralSettingsController : MonoBehaviour
    {
        private const float SENSITIVITY_MAX_MULTIPLIER = 2f;
        private const float SENSITIVITY_MIN_MULTIPLIER = 0.5f;

        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        private CinemachinePOV povCamera;

        private float defaultFirstPersonSpeedX;
        private float defaultFirstPersonSpeedY;
        private float defaultThirdPersonSpeedX;
        private float defaultThirdPersonSpeedY;

        void Awake()
        {
            povCamera = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();

            defaultFirstPersonSpeedX = povCamera.m_HorizontalAxis.m_MaxSpeed;
            defaultFirstPersonSpeedY = povCamera.m_VerticalAxis.m_MaxSpeed;
            defaultThirdPersonSpeedX = thirdPersonCamera.m_XAxis.m_MaxSpeed;
            defaultThirdPersonSpeedY = thirdPersonCamera.m_YAxis.m_MaxSpeed;
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
            float sensitivityMultiplier = Mathf.LerpUnclamped(SENSITIVITY_MIN_MULTIPLIER, SENSITIVITY_MAX_MULTIPLIER, settings.mouseSensitivity);
            povCamera.m_HorizontalAxis.m_MaxSpeed = defaultFirstPersonSpeedX*sensitivityMultiplier;
            povCamera.m_VerticalAxis.m_MaxSpeed = defaultFirstPersonSpeedY*sensitivityMultiplier;
            thirdPersonCamera.m_XAxis.m_MaxSpeed =  defaultThirdPersonSpeedX*sensitivityMultiplier;
            thirdPersonCamera.m_YAxis.m_MaxSpeed = defaultThirdPersonSpeedY*sensitivityMultiplier;

            AudioListener.volume = settings.sfxVolume;
            DCL.Interface.WebInterface.ApplySettings(settings.voiceChatVolume, (int)settings.voiceChatAllow);
        }
    }
}