using Cinemachine;
using DCL.SettingsController;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using GeneralSettings = DCL.SettingsData.GeneralSettings;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace SettingsControllerTests
{
    public class SettingsControllerShould
    {
        private const string TEST_SCENE_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/Settings/SettingsControllers/Tests/TestScenes";
        private const string TEST_SCENE_NAME = "SettingsTestScene";

        private GeneralSettings initGeneralSettings;
        private QualitySettings initQualitySettings;
        private GeneralSettingsController generalSettingsController;
        private QualitySettingsController qualitySettingsController;

        private CinemachineFreeLook freeLookCamera;
        private CinemachineVirtualCamera firstPersonCamera;
        private CinemachinePOV povCamera;
        private Light environmentLight;
        private Volume postProcessVolume;
        private UniversalRenderPipelineAsset urpAsset;
        private FieldInfo lwrpaShadowField = null;
        private FieldInfo lwrpaSoftShadowField = null;
        private FieldInfo lwrpaShadowResolutionField = null;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TEST_SCENE_PATH}/{TEST_SCENE_NAME}.unity", new LoadSceneParameters(LoadSceneMode.Additive));

            SetInitialGeneralSettings();
            SetInitialQualitySettings();
            SetupReferences();
        }

        private void SetInitialGeneralSettings()
        {
            initGeneralSettings = new GeneralSettings()
            {
                mouseSensitivity = 1,
                sfxVolume = 0
            };
        }

        private void SetInitialQualitySettings()
        {
            initQualitySettings = new QualitySettings()
            {
                baseResolution = QualitySettings.BaseResolution.BaseRes_720,
                antiAliasing = MsaaQuality._4x,
                renderScale = 0.1f,
                shadows = false,
                softShadows = true,
                shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._512,
                shadowDistance = 80f,
                cameraDrawDistance = 50.1f,
                bloom = false,
                colorGrading = true,
                detailObjectCullingThreshold = 0,
                enableDetailObjectCulling = true
            };
        }

        private void SetupReferences()
        {
            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            lwrpaShadowField = urpAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            lwrpaSoftShadowField = urpAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            lwrpaShadowResolutionField = urpAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);

            generalSettingsController = GameObject.FindObjectOfType<GeneralSettingsController>();
            qualitySettingsController = GameObject.FindObjectOfType<QualitySettingsController>();
            Assert.IsNotNull(generalSettingsController, "GeneralSettingsController not found in scene");
            Assert.IsNotNull(qualitySettingsController, "QualitySettingsController not found in scene");

            freeLookCamera = generalSettingsController.thirdPersonCamera;
            Assert.IsNotNull(freeLookCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsController.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            povCamera = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            Assert.IsNotNull(povCamera, "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            environmentLight = qualitySettingsController.environmentLight;
            Assert.IsNotNull(environmentLight, "QualitySettingsController: environmentLight reference missing");

            postProcessVolume = qualitySettingsController.postProcessVolume;
            Assert.IsNotNull(postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");

            firstPersonCamera = qualitySettingsController.firstPersonCamera;
            Assert.IsNotNull(firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsController.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            DCL.Settings.i.ApplyGeneralSettings(initGeneralSettings);
            DCL.Settings.i.ApplyQualitySettings(initQualitySettings);

            yield return EditorSceneManager.UnloadSceneAsync(TEST_SCENE_NAME);
        }

        [Test]
        public void HaveItSettingsReferencesSetupCorrectly()
        {
            GeneralSettingsController generalSettingsController = Object.FindObjectOfType<GeneralSettingsController>();
            QualitySettingsController qualitySettingsController = Object.FindObjectOfType<QualitySettingsController>();

            Assert.IsNotNull(generalSettingsController, "GeneralSettingsController not found in scene");
            Assert.IsNotNull(qualitySettingsController, "QualitySettingsController not found in scene");
            Assert.IsNotNull(generalSettingsController.thirdPersonCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsController.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(virtualCamera.GetCinemachineComponent<CinemachinePOV>(), "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            Assert.IsNotNull(qualitySettingsController.environmentLight, "QualitySettingsController: environmentLight reference missing");
            Assert.IsNotNull(qualitySettingsController.postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");
            Assert.IsNotNull(qualitySettingsController.firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsController.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
        }

        [Test]
        public void ApplySoundSfxCorrectly()
        {
            // Arrange
            AudioListener.volume = initGeneralSettings.sfxVolume;

            // Act
            generalSettingsController.UpdateSfxVolume(1f);

            // Assert
            UnityEngine.Assertions.Assert.AreEqual(AudioListener.volume, 1f, "audioListener sfxVolume mismatch");
        }

        [Test]
        public void ApplyMouseSensivityCorrectly()
        {
            // Arrange
            var povSpeed = Mathf.Lerp(GeneralSettingsController.FIRST_PERSON_MIN_SPEED, GeneralSettingsController.FIRST_PERSON_MAX_SPEED, initGeneralSettings.mouseSensitivity);
            povCamera.m_HorizontalAxis.m_MaxSpeed = povSpeed;
            povCamera.m_VerticalAxis.m_MaxSpeed = povSpeed;

            // Act
            generalSettingsController.UpdateMouseSensivity(0f);

            // Assert
            povSpeed = Mathf.Lerp(GeneralSettingsController.FIRST_PERSON_MIN_SPEED, GeneralSettingsController.FIRST_PERSON_MAX_SPEED, 0f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povCamera.m_HorizontalAxis.m_MaxSpeed, povSpeed, "pov (m_HorizontalAxis) mouseSensitivity mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povCamera.m_VerticalAxis.m_MaxSpeed, povSpeed, "pov (m_VerticalAxis) mouseSensitivity mismatch");
            var freeLookXSpeed = Mathf.Lerp(GeneralSettingsController.THIRD_PERSON_X_MIN_SPEED, GeneralSettingsController.THIRD_PERSON_X_MAX_SPEED, 0f);
            var freeLookYSpeed = Mathf.Lerp(GeneralSettingsController.THIRD_PERSON_Y_MIN_SPEED, GeneralSettingsController.THIRD_PERSON_Y_MAX_SPEED, 0f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_XAxis.m_MaxSpeed, freeLookXSpeed, "freeLookCamera (m_XAxis) mouseSensitivity mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_YAxis.m_MaxSpeed, freeLookYSpeed, "freeLookCamera (m_YAxis) mouseSensitivity mismatch");
        }

        [Test]
        public void ApplyBloomCorrectly()
        {
            // Arrange
            if (!postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
                return;

            bloom.active = initQualitySettings.bloom;

            // Act
            qualitySettingsController.UpdateBloom(true);

            // Assert
            Assert.AreEqual(bloom.active, true, "bloom mismatch");
        }

        [Test]
        public void ApplyAntialiasingCorrectly()
        {
            // Arrange
            if (urpAsset == null)
                return;

            urpAsset.msaaSampleCount = (int)initQualitySettings.antiAliasing;

            // Act
            qualitySettingsController.UpdateAntiAliasing((int)MsaaQuality._8x);

            // Assert
            Assert.AreEqual(urpAsset.msaaSampleCount, (int)MsaaQuality._8x, "antiAliasing mismatch");
        }

        [Test]
        public void ApplyColorGradingCorrectly()
        {
            // Arrange
            if (!postProcessVolume.profile.TryGet<Tonemapping>(out Tonemapping toneMapping))
                return;

            toneMapping.active = initQualitySettings.colorGrading;

            // Act
            qualitySettingsController.UpdateColorGrading(false);

            // Assert
            Assert.AreEqual(toneMapping.active, false, "colorGrading mismatch");
        }

        [Test]
        public void ApplyDrawDistanceCorrectly()
        {
            // Arrange
            if (freeLookCamera == null || firstPersonCamera == null)
                return;

            freeLookCamera.m_Lens.FarClipPlane = initQualitySettings.cameraDrawDistance;
            firstPersonCamera.m_Lens.FarClipPlane = initQualitySettings.cameraDrawDistance;

            // Act
            qualitySettingsController.UpdateDrawDistance(5f);

            // Assert
            Assert.AreEqual(firstPersonCamera.m_Lens.FarClipPlane, 5f, "cameraDrawDistance (firstPersonCamera) mismatch");
            Assert.AreEqual(freeLookCamera.m_Lens.FarClipPlane, 5f, "cameraDrawDistance (freeLookCamera) mismatch");
        }

        [Test]
        public void ApplyRenderingScaleCorrectly()
        {
            // Arrange
            if (urpAsset == null)
                return;

            urpAsset.renderScale = initQualitySettings.renderScale;

            // Act
            qualitySettingsController.UpdateRenderingScale(0.3f);

            // Assert
            Assert.AreEqual(urpAsset.renderScale, 0.3f, "renderScale mismatch");
        }

        [Test]
        public void ApplyShadowCorrectly()
        {
            // Arrange
            if (urpAsset == null)
                return;

            lwrpaShadowField?.SetValue(urpAsset, initQualitySettings.shadows);

            // Act
            qualitySettingsController.UpdateShadows(true);

            // Assert
            Assert.AreEqual(urpAsset.supportsMainLightShadows, true, "shadows mismatch");
        }

        [Test]
        public void ApplySoftShadowsCorrectly()
        {
            // Arrange
            if (urpAsset == null)
                return;

            lwrpaSoftShadowField?.SetValue(urpAsset, initQualitySettings.softShadows);

            // Act
            qualitySettingsController.UpdateSoftShadows(false);

            // Assert
            Assert.AreEqual(urpAsset.supportsSoftShadows, false, "softShadows mismatch");
        }

        [Test]
        public void ApplyShadowResolutionCorrectly()
        {
            // Arrange
            if (urpAsset == null)
                return;

            lwrpaShadowResolutionField?.SetValue(urpAsset, initQualitySettings.shadowResolution);

            // Act
            qualitySettingsController.UpdateShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._2048);

            // Assert
            Assert.AreEqual(urpAsset.mainLightShadowmapResolution, (int)UnityEngine.Rendering.Universal.ShadowResolution._2048, "shadowResolution mismatch");
        }

        [Test]
        public void ApplyShadowDistanceCorrectly()
        {
            // Arrange
            if (urpAsset == null)
                return;

            urpAsset.shadowDistance = initQualitySettings.shadowDistance;

            // Act
            qualitySettingsController.UpdateShadowDistance(90f);

            // Assert
            Assert.AreEqual(urpAsset.shadowDistance, 90f, "shadowDistance mismatch");
        }
    }
}