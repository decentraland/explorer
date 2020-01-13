using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.PostProcessing;
using NUnit.Framework;
using Cinemachine;
using DCL.SettingsHUD;

using QualitySettings = DCL.SettingsHUD.QualitySettings;

namespace Tests
{
    public class GeneralSettingsShould : TestsBase
    {
        QualitySettings testQualitySettings;
        GeneralSettings testGeneralSettings;

        CinemachineFreeLook freeLookCamera;
        CinemachineVirtualCamera firstPersonCamera;
        CinemachinePOV povCamera;
        AudioListener audioListener;
        Light environmentLight;
        PostProcessVolume postProcessVolume;
        LightweightRenderPipelineAsset lwrpAsset;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            testQualitySettings = new QualitySettings()
            {
                textureQuality = QualitySettings.TextureQuality.HalfRes,
                antiAliasing = MsaaQuality._4x,
                renderScale = 0.1f,
                shadows = false,
                softShadows = true,
                shadowResolution = UnityEngine.Rendering.LWRP.ShadowResolution._512,
                cameraDrawDistance = 50.1f,
                bloom = false,
                colorGrading = true
            };

            testGeneralSettings = new GeneralSettings()
            {
                mouseSensitivity = 1,
                sfxVolume = 0
            };

            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);

            yield return InitScene();
        }

        public IEnumerator InitScene()
        {
            if (!sceneInitialized)
            {
                yield return InitUnityScene("InitialScene");
                GameObject.DestroyImmediate(DCL.WSSController.i.gameObject);
                sceneInitialized = true;
            }

            SetUp_Camera();
            yield return SetUp_SceneController();
            yield return SetUp_CharacterController();
        }

        [UnityTest]
        public IEnumerator ApplyCorrectly()
        {
            // NOTE: settings here were set before scene loading
            SetupAndCheckReferences();

            CheckIfTestQualitySettingsAreSet();
            CheckIfTestGeneralSettingsAreSet();

            // NOTE: wait for next frame
            yield return null;

            CheckIfQualitySettingsAreApplied();
            CheckIfGeneralSettingsAreApplied();

            // NOTE: change settings in runtime
            testQualitySettings = DCL.Settings.i.qualitySettingsPresets[0];
            testGeneralSettings = new GeneralSettings()
            {
                sfxVolume = 1,
                mouseSensitivity = 0
            };
            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);

            CheckIfTestQualitySettingsAreSet();
            CheckIfTestGeneralSettingsAreSet();

            // NOTE: wait for next frame
            yield return null;

            CheckIfQualitySettingsAreApplied();
            CheckIfGeneralSettingsAreApplied();

            yield break;
        }

        public void SetupAndCheckReferences()
        {
            lwrpAsset = GraphicsSettings.renderPipelineAsset as LightweightRenderPipelineAsset;
            GeneralSettingsController generalSettingsController = GameObject.FindObjectOfType<GeneralSettingsController>();
            QualitySettingsController qualitySettingsController = GameObject.FindObjectOfType<QualitySettingsController>();

            Assert.IsNotNull(generalSettingsController, "GeneralSettingsController not found in scene");
            Assert.IsNotNull(qualitySettingsController, "QualitySettingsController not found in scene");

            freeLookCamera = generalSettingsController.thirdPersonCamera;
            Assert.IsNotNull(freeLookCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsController.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            povCamera = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            Assert.IsNotNull(povCamera, "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            audioListener = generalSettingsController.audioListener;
            Assert.IsNotNull(audioListener, "GeneralSettingsController: AudioListener reference missing");

            environmentLight = qualitySettingsController.environmentLight;
            Assert.IsNotNull(environmentLight, "QualitySettingsController: environmentLight reference missing");

            postProcessVolume = qualitySettingsController.postProcessVolume;
            Assert.IsNotNull(postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");

            firstPersonCamera = qualitySettingsController.firstPersonCamera;
            Assert.IsNotNull(firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsController.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
        }

        private void CheckIfTestQualitySettingsAreSet()
        {
            Assert.IsTrue(DCL.Settings.i.qualitySettings.Equals(testQualitySettings), "Quality Setting missmatch");
        }

        private void CheckIfTestGeneralSettingsAreSet()
        {
            Assert.IsTrue(DCL.Settings.i.generalSettings.Equals(testGeneralSettings), "General Settings missmatch");
        }

        private void CheckIfQualitySettingsAreApplied()
        {
            Assert.IsTrue(lwrpAsset.msaaSampleCount == (int)DCL.Settings.i.qualitySettings.antiAliasing, "antiAliasing missmatch");
            Assert.IsTrue(lwrpAsset.renderScale == DCL.Settings.i.qualitySettings.renderScale, "renderScale missmatch");
            Assert.IsTrue(lwrpAsset.supportsMainLightShadows == DCL.Settings.i.qualitySettings.shadows, "shadows missmatch");
            Assert.IsTrue(lwrpAsset.supportsSoftShadows == DCL.Settings.i.qualitySettings.softShadows, "softShadows missmatch");
            Assert.IsTrue(lwrpAsset.mainLightShadowmapResolution == (int)DCL.Settings.i.qualitySettings.shadowResolution, "shadowResolution missmatch");

            LightShadows shadowType = LightShadows.None;
            if (DCL.Settings.i.qualitySettings.shadows)
            {
                shadowType = DCL.Settings.i.qualitySettings.softShadows ? LightShadows.Soft : LightShadows.Hard;
            }
            Assert.IsTrue(environmentLight.shadows == shadowType, "shadows (environmentLight) missmatch");
            Bloom bloom;
            if (postProcessVolume.profile.TryGetSettings(out bloom))
            {
                Assert.IsTrue(bloom.enabled.value == DCL.Settings.i.qualitySettings.bloom, "bloom missmatch");
            }
            ColorGrading colorGrading;
            if (postProcessVolume.profile.TryGetSettings(out colorGrading))
            {
                Assert.IsTrue(colorGrading.enabled.value == DCL.Settings.i.qualitySettings.colorGrading, "colorGrading missmatch");
            }
            Assert.IsTrue(firstPersonCamera.m_Lens.FarClipPlane == DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (firstPersonCamera) missmatch");
            Assert.IsTrue(freeLookCamera.m_Lens.FarClipPlane == DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (freeLookCamera) missmatch");
        }

        private void CheckIfGeneralSettingsAreApplied()
        {
            Assert.IsTrue(freeLookCamera.m_XAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_XAxis) mouseSensitivity missmatch");
            Assert.IsTrue(freeLookCamera.m_YAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_YAxis) mouseSensitivity missmatch");
            Assert.IsTrue(povCamera.m_HorizontalAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_HorizontalAxis) mouseSensitivity missmatch");
            Assert.IsTrue(povCamera.m_VerticalAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_VerticalAxis) mouseSensitivity missmatch");
            Assert.IsTrue(audioListener.enabled == (DCL.Settings.i.generalSettings.sfxVolume != 0), "audioListener sfxVolume missmatch");
        }
    }
}