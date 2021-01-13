using Cinemachine;
using DCL.Interface;
using UnityEngine;

namespace DCL.SettingsController
{
    public interface IGeneralSettingsController
    {
        void UpdateAllowVoiceChat(int isActive);
        void UpdateVoiceChatVolume(float newValue);
        void UpdateMouseSensivity(float newValue);
        void UpdateSfxVolume(float newValue);
    }

    public class GeneralSettingsController : MonoBehaviour, IGeneralSettingsController
    {
        internal const float FIRST_PERSON_MIN_SPEED = 25f;
        internal const float FIRST_PERSON_MAX_SPEED = 350f;
        internal const float THIRD_PERSON_X_MIN_SPEED = 100f;
        internal const float THIRD_PERSON_X_MAX_SPEED = 450f;
        internal const float THIRD_PERSON_Y_MIN_SPEED = 0.5f;
        internal const float THIRD_PERSON_Y_MAX_SPEED = 3f;

        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
        private CinemachinePOV povCamera;

        public static GeneralSettingsController i { get; private set; }

        private void Awake()
        {
            i = this;

            povCamera = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        public void UpdateAllowVoiceChat(int isActive)
        {
            WebInterface.ApplySettings(Settings.i.generalSettings.voiceChatVolume, isActive);
        }

        public void UpdateVoiceChatVolume(float newValue)
        {
            WebInterface.ApplySettings(newValue, (int)Settings.i.generalSettings.voiceChatAllow);
        }

        public void UpdateMouseSensivity(float newValue)
        {
            var povSpeed = Mathf.Lerp(FIRST_PERSON_MIN_SPEED, FIRST_PERSON_MAX_SPEED, newValue);
            povCamera.m_HorizontalAxis.m_MaxSpeed = povSpeed;
            povCamera.m_VerticalAxis.m_MaxSpeed = povSpeed;
            thirdPersonCamera.m_XAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_X_MIN_SPEED, THIRD_PERSON_X_MAX_SPEED, newValue);
            thirdPersonCamera.m_YAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_Y_MIN_SPEED, THIRD_PERSON_Y_MAX_SPEED, newValue);
        }

        public void UpdateSfxVolume(float newValue)
        {
            AudioListener.volume = newValue;
        }
    }
}