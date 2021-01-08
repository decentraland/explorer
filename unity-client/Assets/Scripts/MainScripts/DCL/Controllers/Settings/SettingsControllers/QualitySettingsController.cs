using Cinemachine;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace DCL.SettingsController
{
    public class QualitySettingsController : MonoBehaviour
    {
        public Light environmentLight = null;

        public Volume postProcessVolume = null;
        public CinemachineFreeLook thirdPersonCamera = null;
        public CinemachineVirtualCamera firstPersonCamera = null;

        public CullingControllerSettingsData cullingControllerSettingsData = null;

        public static QualitySettingsController i { get; private set; }

        private void Awake()
        {
            i = this;
        }

        void Start()
        {
            ApplyQualitySettings(Settings.i.qualitySettings);
        }

        void OnEnable()
        {
            Settings.i.OnQualitySettingsChanged += ApplyQualitySettings;
        }

        void OnDisable()
        {
            Settings.i.OnQualitySettingsChanged -= ApplyQualitySettings;
        }

        void ApplyQualitySettings(QualitySettings qualitySettings)
        {

        }
    }
}