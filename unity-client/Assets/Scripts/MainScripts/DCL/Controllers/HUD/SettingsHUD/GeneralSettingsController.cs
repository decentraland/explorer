﻿using Cinemachine;
using UnityEngine;

namespace DCL.SettingsHUD
{
    public class GeneralSettingsController : MonoBehaviour
    {
        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;
        public AudioListener audioListener;

        private CinemachinePOV povCamera;

        void Awake()
        {
            povCamera = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
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

        void ApplyGeneralSettings(GeneralSettings settings)
        {
            thirdPersonCamera.m_XAxis.m_AccelTime = settings.mouseSensitivity;
            thirdPersonCamera.m_YAxis.m_AccelTime = settings.mouseSensitivity;
            povCamera.m_HorizontalAxis.m_AccelTime = settings.mouseSensitivity;
            povCamera.m_VerticalAxis.m_AccelTime = settings.mouseSensitivity;
            audioListener.enabled = settings.sfxVolume != 0;
        }
    }
}