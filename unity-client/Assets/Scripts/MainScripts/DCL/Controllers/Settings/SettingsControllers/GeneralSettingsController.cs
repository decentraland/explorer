using Cinemachine;
using UnityEngine;

namespace DCL.SettingsController
{
    public class GeneralSettingsController : MonoBehaviour
    {
        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        public static GeneralSettingsController i { get; private set; }

        private void Awake()
        {
            i = this;
        }
    }
}