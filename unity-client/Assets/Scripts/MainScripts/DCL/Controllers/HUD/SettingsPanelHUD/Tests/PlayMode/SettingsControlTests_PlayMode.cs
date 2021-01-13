using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using DCL.SettingsPanelHUD.Controls;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace SettingsControlsTests
{
    public class SettingsControlTests_PlayMode
    {
        private const string CONTROL_VIEW_PREFAB_PATH = "Controls/{controlType}SettingsControlTemplate";

        private SettingsControlView newControlView;
        private SettingsControlModel newControlModel;
        private IGeneralSettingsController generalSettingsRefMock;
        private IQualitySettingsController qualitySettingsRefMock;

        [SetUp]
        public void SetUp()
        {
            generalSettingsRefMock = Substitute.For<IGeneralSettingsController>();
            qualitySettingsRefMock = Substitute.For<IQualitySettingsController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(newControlModel);

            if (newControlView != null)
                Object.Destroy(newControlView.gameObject);
        }

        [UnityTest]
        public IEnumerator ChangeAllowVoiceChatCorrectly()
        {
            // Arrange
            string[] labels = { "All users", "Verified users", "Friends" };
            yield return CreateSpinBoxSettingsControl<AllowVoiceChatControlController>(labels);

            // Act
            DCL.SettingsData.GeneralSettings.VoiceChatAllow newValue = DCL.SettingsData.GeneralSettings.VoiceChatAllow.FRIENDS_ONLY;
            ((SpinBoxSettingsControlView)newControlView).spinBoxControl.value = (int)newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentGeneralSettings.voiceChatAllow, newValue, "voiceChatAllow mismatch");
            generalSettingsRefMock.Received(1).UpdateAllowVoiceChat((int)newValue);
        }

        [UnityTest]
        public IEnumerator ChangeAntialiasingChatCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<AntiAliasingControlController>(0, 3, true);

            // Act
            UnityEngine.Rendering.Universal.MsaaQuality newValue = UnityEngine.Rendering.Universal.MsaaQuality._8x;
            ((SliderSettingsControlView)newControlView).sliderControl.value = (int)newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.antiAliasing, newValue, "antiAliasing mismatch");
            qualitySettingsRefMock.Received(1).UpdateAntiAliasing((int)newValue);
        }

        [UnityTest]
        public IEnumerator ChangeBaseResolutionCorrectly()
        {
            // Arrange
            string[] labels = { "Match 720p", "Match 1080p", "Unlimited" };
            yield return CreateSpinBoxSettingsControl<BaseResolutionControlController>(labels);

            // Act
            DCL.SettingsData.QualitySettings.BaseResolution newValue = DCL.SettingsData.QualitySettings.BaseResolution.BaseRes_1080;
            ((SpinBoxSettingsControlView)newControlView).spinBoxControl.value = (int)newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.baseResolution, newValue, "baseResolution mismatch");
            qualitySettingsRefMock.Received().UpdateBaseResolution(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeBloomCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<BloomControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.bloom, newValue, "bloom mismatch");
            qualitySettingsRefMock.Received(1).UpdateBloom(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeColorGradingCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<ColorGradingControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.colorGrading, newValue, "colorGrading mismatch");
            qualitySettingsRefMock.Received(1).UpdateColorGrading(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeDetailObjectCullingCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<DetailObjectCullingControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.enableDetailObjectCulling, newValue, "enableDetailObjectCulling mismatch");
            qualitySettingsRefMock.Received(1).UpdateDetailObjectCulling(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeDetailObjectCullingSizeCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<DetailObjectCullingSizeControlController>(0, 100, true);

            // Act
            float newValue = 20f;
            ((SliderSettingsControlView)newControlView).sliderControl.value = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.detailObjectCullingThreshold, newValue, "detailObjectCullingThreshold mismatch");
            qualitySettingsRefMock.Received(1).UpdateDetailObjectCullingSize(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeDrawDistanceCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<DrawDistanceControlController>(40, 100, true);

            // Act
            float newValue = 50f;
            ((SliderSettingsControlView)newControlView).sliderControl.value = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.cameraDrawDistance, newValue, "cameraDrawDistance mismatch");
            qualitySettingsRefMock.Received(1).UpdateDrawDistance(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeFPSLimitCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<FPSLimitControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.fpsCap, newValue, "currentQualitySetting mismatch");
            qualitySettingsRefMock.Received(1).UpdateFPSLimit(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeMouseSensivityCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<MouseSensivityControlController>(1, 100, true);

            // Act
            float newValue = 80f;
            float remapedNewValue = ((MouseSensivityControlController)newControlModel.controlController).RemapMouseSensitivityTo01(newValue);
            ((SliderSettingsControlView)newControlView).sliderControl.value = newValue;

            // Assert
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(
                newControlModel.controlController.currentGeneralSettings.mouseSensitivity,
                remapedNewValue,
                "mouseSensitivity mismatch");

            generalSettingsRefMock.Received(1).UpdateMouseSensivity(remapedNewValue);
        }

        [UnityTest]
        public IEnumerator ChangeMuteSoundCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<MuteSoundControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentGeneralSettings.sfxVolume, newValue ? 1f : 0f, "sfxVolume mismatch");
            generalSettingsRefMock.Received(1).UpdateSfxVolume(newValue ? 1f : 0f);
        }

        [UnityTest]
        public IEnumerator ChangeRenderingScaleCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<RenderingScaleControlController>(0, 1, false);

            // Act
            float newValue = 0.5f;
            ((SliderSettingsControlView)newControlView).sliderControl.value = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.renderScale, newValue, "renderScale mismatch");
            qualitySettingsRefMock.Received(1).UpdateRenderingScale(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeShadowsCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<ShadowControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.shadows, newValue, "shadows mismatch");
            qualitySettingsRefMock.Received(1).UpdateShadows(newValue);
            Assert.AreEqual(CommonSettingsScriptableObjects.shadowsDisabled.Get(), !newValue);
        }

        [UnityTest]
        public IEnumerator ChangeShadowDistanceCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<ShadowDistanceControlController>(30, 100, true);

            // Act
            float newValue = 50f;
            ((SliderSettingsControlView)newControlView).sliderControl.value = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.shadowDistance, newValue, "shadowDistance mismatch");
            qualitySettingsRefMock.Received(1).UpdateShadowDistance(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeShadowresolutionCorrectly()
        {
            // Arrange
            string[] labels = { "256", "512", "1024", "2048", "4096" };
            yield return CreateSpinBoxSettingsControl<ShadowResolutionControlController>(labels);

            // Act
            int newValue = 4;
            UnityEngine.Rendering.Universal.ShadowResolution newValueFormatted = (UnityEngine.Rendering.Universal.ShadowResolution)(256 << newValue);
            ((SpinBoxSettingsControlView)newControlView).spinBoxControl.value = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.shadowResolution, newValueFormatted, "shadowResolution mismatch");
            qualitySettingsRefMock.Received(1).UpdateShadowResolution(newValueFormatted);
        }

        [UnityTest]
        public IEnumerator ChangeSoftShadowsCorrectly()
        {
            // Arrange
            yield return CreateToggleSettingsControl<SoftShadowsControlController>();

            // Act
            bool newValue = true;
            ((ToggleSettingsControlView)newControlView).toggleControl.isOn = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentQualitySetting.softShadows, newValue, "softShadows mismatch");
            qualitySettingsRefMock.Received(1).UpdateSoftShadows(newValue);
        }

        [UnityTest]
        public IEnumerator ChangeVoiceChatVolumeCorrectly()
        {
            // Arrange
            yield return CreateSliderSettingsControl<VoiceChatVolumeControlController>(0, 100, true);

            // Act
            float newValue = 90f;
            ((SliderSettingsControlView)newControlView).sliderControl.value = newValue;

            // Assert
            Assert.AreEqual(newControlModel.controlController.currentGeneralSettings.voiceChatVolume, newValue * 0.01f, "voiceChatVolume mismatch");
            generalSettingsRefMock.Received(1).UpdateVoiceChatVolume(newValue * 0.01f);
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

            newControlView.Initialize(
                newControlModel,
                newControlModel.controlController,
                generalSettingsRefMock,
                qualitySettingsRefMock);

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

            newControlView.Initialize(
                newControlModel,
                newControlModel.controlController,
                generalSettingsRefMock,
                qualitySettingsRefMock);

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

            newControlView.Initialize(
                newControlModel,
                newControlModel.controlController,
                generalSettingsRefMock,
                qualitySettingsRefMock);

            yield return null;
        }
    }
}
