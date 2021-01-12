using Cinemachine;
using DCL.SettingsController;
using DCL.SettingsPanelHUD.Controls;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using GeneralSettings = DCL.SettingsData.GeneralSettings;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace SettingsControlsTests
{
    public class SettingsControlTests_PlayMode : IntegrationTestSuite_Legacy
    {
        private SettingsControlView newControlView;
        private SettingsControlModel newControlModel;
        private SettingsControlController newControlController;

        private const string CONTROL_VIEW_PREFAB_PATH = "Controls/{controlType}SettingsControlTemplate";

        private GeneralSettings initGeneralSettings;
        private GeneralSettings testGeneralSettings;
        private QualitySettings initQualitySettings;
        private QualitySettings testQualitySettings;

        private CinemachineFreeLook freeLookCamera;
        private CinemachineVirtualCamera firstPersonCamera;
        private CinemachinePOV povCamera;

        private Light environmentLight;

        private Volume postProcessVolume;
        private UniversalRenderPipelineAsset urpAsset;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            SetInitialGeneralSettings();
            SetInitialQualitySettings();
            SetupReferences();
        }

        private void SetInitialGeneralSettings()
        {
            testGeneralSettings = new GeneralSettings()
            {
                mouseSensitivity = 1,
                sfxVolume = 0
            };

            initGeneralSettings = DCL.Settings.i.generalSettings;
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);
            Assert.IsTrue(DCL.Settings.i.generalSettings.Equals(testGeneralSettings), "General Settings mismatch");
        }

        private void SetInitialQualitySettings()
        {
            testQualitySettings = new QualitySettings()
            {
                baseResolution = QualitySettings.BaseResolution.BaseRes_720,
                antiAliasing = MsaaQuality._4x,
                renderScale = 0.1f,
                shadows = false,
                softShadows = true,
                shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._512,
                cameraDrawDistance = 50.1f,
                bloom = false,
                colorGrading = true,
                detailObjectCullingThreshold = 0,
                enableDetailObjectCulling = true
            };

            initQualitySettings = DCL.Settings.i.qualitySettings;
            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            Assert.IsTrue(DCL.Settings.i.qualitySettings.Equals(testQualitySettings), "Quality Setting mismatch");
        }

        private void SetupReferences()
        {
            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            GeneralSettingsReferences generalSettingsReferences = GameObject.FindObjectOfType<GeneralSettingsReferences>();
            QualitySettingsReferences qualitySettingsReferences = GameObject.FindObjectOfType<QualitySettingsReferences>();

            Assert.IsNotNull(generalSettingsReferences, "GeneralSettingsController not found in scene");
            Assert.IsNotNull(qualitySettingsReferences, "QualitySettingsController not found in scene");

            freeLookCamera = generalSettingsReferences.thirdPersonCamera;
            Assert.IsNotNull(freeLookCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsReferences.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            povCamera = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            Assert.IsNotNull(povCamera, "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            environmentLight = qualitySettingsReferences.environmentLight;
            Assert.IsNotNull(environmentLight, "QualitySettingsController: environmentLight reference missing");

            postProcessVolume = qualitySettingsReferences.postProcessVolume;
            Assert.IsNotNull(postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");

            firstPersonCamera = qualitySettingsReferences.firstPersonCamera;
            Assert.IsNotNull(firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsReferences.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
        }

        protected override IEnumerator TearDown()
        {
            Object.Destroy(newControlController);
            Object.Destroy(newControlModel);

            if (newControlView != null)
                Object.Destroy(newControlView.gameObject);

            DCL.Settings.i.ApplyGeneralSettings(initGeneralSettings);
            DCL.Settings.i.ApplyQualitySettings(initQualitySettings);
            yield return base.TearDown();
        }

        [Test]
        public void HaveItSettingsReferencesSetupCorrectly()
        {
            GeneralSettingsReferences generalSettingsController = Object.FindObjectOfType<GeneralSettingsReferences>();
            QualitySettingsReferences qualitySettingsController = Object.FindObjectOfType<QualitySettingsReferences>();

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
        public void HaveQualityPresetSetCorrectly()
        {
            Assert.IsTrue(DCL.Settings.i.qualitySettingsPresets.Length > 0, "QualitySettingsData: No presets created");
            Assert.IsTrue(DCL.Settings.i.qualitySettingsPresets.defaultIndex > 0
                          && DCL.Settings.i.qualitySettingsPresets.defaultIndex < DCL.Settings.i.qualitySettingsPresets.Length, "QualitySettingsData: Wrong default preset index");
        }

        [UnityTest]
        public IEnumerator ApplyBloomCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<BloomControlController>();

            // Act
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = true;

            // Assert
            if (postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
            {
                Assert.IsTrue(bloom.active == DCL.Settings.i.qualitySettings.bloom, "bloom mismatch");
            }
        }

        [UnityTest]
        public IEnumerator ApplyMuteSoundCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<MuteSoundControlController>();

            // Act
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = true;

            // Assert
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(AudioListener.volume, DCL.Settings.i.generalSettings.sfxVolume, "audioListener sfxVolume mismatch");
        }

        [UnityTest]
        public IEnumerator ApplyMouseSensivityCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<MouseSensivityControlController>(1f, 100f, true);

            // Act
            ((SliderSettingsControlView)newControlView).sliderControl.value = 0f;

            // Assert
            var povSpeed = Mathf.Lerp(MouseSensivityControlController.FIRST_PERSON_MIN_SPEED, MouseSensivityControlController.FIRST_PERSON_MAX_SPEED, DCL.Settings.i.generalSettings.mouseSensitivity);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povCamera.m_HorizontalAxis.m_MaxSpeed, povSpeed, "pov (m_HorizontalAxis) mouseSensitivity mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povCamera.m_VerticalAxis.m_MaxSpeed, povSpeed, "pov (m_VerticalAxis) mouseSensitivity mismatch");
            var freeLookXSpeed = Mathf.Lerp(MouseSensivityControlController.THIRD_PERSON_X_MIN_SPEED, MouseSensivityControlController.THIRD_PERSON_X_MAX_SPEED, DCL.Settings.i.generalSettings.mouseSensitivity);
            var freeLookYSpeed = Mathf.Lerp(MouseSensivityControlController.THIRD_PERSON_Y_MIN_SPEED, MouseSensivityControlController.THIRD_PERSON_Y_MAX_SPEED, DCL.Settings.i.generalSettings.mouseSensitivity);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_XAxis.m_MaxSpeed, freeLookXSpeed, "freeLookCamera (m_XAxis) mouseSensitivity mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_YAxis.m_MaxSpeed, freeLookYSpeed, "freeLookCamera (m_YAxis) mouseSensitivity mismatch");
        }

        [UnityTest]
        public IEnumerator ApplyAntialiasingCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<AntiAliasingControlController>(0f, 1f, true);

            // Act
            ((SliderSettingsControlView)newControlView).sliderControl.value = 0f;

            // Assert
            Assert.IsTrue(urpAsset.msaaSampleCount == (int)DCL.Settings.i.qualitySettings.antiAliasing, "antiAliasing mismatch");
        }

        [UnityTest]
        public IEnumerator ApplyColorGradingCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<ColorGradingControlController>();

            // Act
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = false;

            // Assert
            if (postProcessVolume.profile.TryGet<Tonemapping>(out Tonemapping toneMapping))
            {
                Assert.IsTrue(toneMapping.active == DCL.Settings.i.qualitySettings.colorGrading, "colorGrading mismatch");
            }
        }

        [UnityTest]
        public IEnumerator ApplyDrawDistanceCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<DrawDistanceControlController>(40f, 100f, true);

            // Act
            ((SliderSettingsControlView)newControlView).sliderControl.value = 5f;

            // Assert
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(firstPersonCamera.m_Lens.FarClipPlane, DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (firstPersonCamera) mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_Lens.FarClipPlane, DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (freeLookCamera) mismatch");
        }

        [UnityTest]
        public IEnumerator RenderingScaleCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<RenderingScaleControlController>(0f, 1f, false);

            // Act
            ((SliderSettingsControlView)newControlView).sliderControl.value = 0.3f;

            // Assert
            Assert.IsTrue(urpAsset.renderScale == DCL.Settings.i.qualitySettings.renderScale, "renderScale mismatch");
        }

        [UnityTest]
        public IEnumerator ApplyShadowCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<ShadowControlController>();

            // Act
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = true;

            // Assert
            Assert.IsTrue(urpAsset.supportsMainLightShadows == DCL.Settings.i.qualitySettings.shadows, "shadows mismatch");

            LightShadows shadowType = LightShadows.None;
            if (DCL.Settings.i.qualitySettings.shadows)
            {
                shadowType = DCL.Settings.i.qualitySettings.softShadows ? LightShadows.Soft : LightShadows.Hard;
            }

            Assert.IsTrue(environmentLight.shadows == shadowType, "shadows (environmentLight) mismatch");
        }

        [UnityTest]
        public IEnumerator ApplySoftShadowsCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<SoftShadowsControlController>();

            // Act
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = false;

            // Assert
            Assert.IsTrue(urpAsset.supportsSoftShadows == DCL.Settings.i.qualitySettings.softShadows, "softShadows mismatch");

            LightShadows shadowType = LightShadows.None;
            if (DCL.Settings.i.qualitySettings.shadows)
            {
                shadowType = DCL.Settings.i.qualitySettings.softShadows ? LightShadows.Soft : LightShadows.Hard;
            }

            Assert.IsTrue(environmentLight.shadows == shadowType, "shadows (environmentLight) mismatch");
        }

        [UnityTest]
        public IEnumerator ApplyShadowResolutionCorrectly()
        {
            // Arrange
            string[] labels = { "256", "512", "1024", "2048", "4096" };
            yield return CreateSpinBoxSettingsControl<ShadowResolutionControlController>(labels);

            // Act
            ((SpinBoxSettingsControlView)newControlView).spinBoxControl.value = (int)UnityEngine.Rendering.Universal.ShadowResolution._2048;

            // Assert
            Assert.IsTrue(urpAsset.mainLightShadowmapResolution == (int)DCL.Settings.i.qualitySettings.shadowResolution, "shadowResolution mismatch");
        }

        private IEnumerator CreateToggleSettingsControl<T>() where T : SettingsControlController
        {
            newControlView = Object.Instantiate((GameObject)Resources.Load(CONTROL_VIEW_PREFAB_PATH.Replace("{controlType}", "Toggle"))).GetComponent<ToggleSettingsControlView>();
            newControlModel = ScriptableObject.CreateInstance<ToggleControlModel>();
            newControlModel.title = "TestToggleControl";
            newControlModel.controlPrefab = newControlView;
            newControlModel.controlController = ScriptableObject.CreateInstance<T>();
            newControlModel.flagsThatDeactivateMe = new List<BooleanVariable>();
            newControlModel.flagsThatDisableMe = new List<BooleanVariable>();
            newControlModel.isBeta = false;

            newControlView.Initialize(newControlModel, newControlModel.controlController);
            yield return null;
        }

        private IEnumerator CreateSliderSettingsControl<T>(
            float sliderMinValue,
            float sliderMaxValue,
            bool sliderWholeNumbers) where T : SettingsControlController
        {
            newControlView = Object.Instantiate((GameObject)Resources.Load(CONTROL_VIEW_PREFAB_PATH.Replace("{controlType}", "Slider"))).GetComponent<SliderSettingsControlView>();
            newControlModel = ScriptableObject.CreateInstance<SliderControlModel>();
            newControlModel.title = "TestSliderControl";
            newControlModel.controlPrefab = newControlView;
            newControlModel.controlController = ScriptableObject.CreateInstance<T>();
            newControlModel.flagsThatDeactivateMe = new List<BooleanVariable>();
            newControlModel.flagsThatDisableMe = new List<BooleanVariable>();
            newControlModel.isBeta = false;
            ((SliderControlModel)newControlModel).sliderMinValue = sliderMinValue;
            ((SliderControlModel)newControlModel).sliderMaxValue = sliderMaxValue;
            ((SliderControlModel)newControlModel).sliderWholeNumbers = sliderWholeNumbers;

            newControlView.Initialize(newControlModel, newControlModel.controlController);
            yield return null;
        }

        private IEnumerator CreateSpinBoxSettingsControl<T>(string[] spinBoxLabels) where T : SettingsControlController
        {
            newControlView = Object.Instantiate((GameObject)Resources.Load(CONTROL_VIEW_PREFAB_PATH.Replace("{controlType}", "SpinBox"))).GetComponent<SpinBoxSettingsControlView>();
            newControlModel = ScriptableObject.CreateInstance<SpinBoxControlModel>();
            newControlModel.title = "TestSpinBoxControl";
            newControlModel.controlPrefab = newControlView;
            newControlModel.controlController = ScriptableObject.CreateInstance<T>();
            newControlModel.flagsThatDeactivateMe = new List<BooleanVariable>();
            newControlModel.flagsThatDisableMe = new List<BooleanVariable>();
            newControlModel.isBeta = false;
            ((SpinBoxControlModel)newControlModel).spinBoxLabels = spinBoxLabels;

            newControlView.Initialize(newControlModel, newControlModel.controlController);
            yield return null;
        }
    }
}
