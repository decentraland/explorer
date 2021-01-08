using Cinemachine;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

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
    }
}