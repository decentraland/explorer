using System.Collections;
using System.Collections.Generic;
using DCL.Rendering;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace CullingControllerTests
{
    public class CullingControllerShould
    {
        public CullingController cullingController;

        [SetUp]
        public void SetUp()
        {
            cullingController = new CullingController(GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset, new CullingControllerSettings());
        }

        [TearDown]
        public void TearDown()
        {
            cullingController.Dispose();
        }

        [Test]
        public void ReturnCopyWhenGetSettingsIsCalled()
        {
            // Ensure settings never return null
            var settings = cullingController.GetSettings();
            Assert.IsNotNull(settings, "Settings should never be null!");

            var prevAnimationCulling = settings.enableAnimationCulling;

            // This is needed because SetSettings sets dirty flag and other stuff. We don't want direct access.
            // Settings is not a property because to make the SetSetting performance hit more obvious.
            settings.enableAnimationCulling = !settings.enableAnimationCulling;
            settings = cullingController.GetSettings();
            Assert.IsTrue(settings.enableAnimationCulling == prevAnimationCulling, "GetSettings should return a copy!");
        }

        [Test]
        public void SetSettingsProperly()
        {
            // Ensure settings never return null
            var settings = cullingController.GetSettings();
            Assert.IsNotNull(settings, "Settings should never be null!");

            var prevAnimationCulling = settings.enableAnimationCulling;
            var prevRendererProfile = settings.rendererProfile;
            var prevSkinnedProfile = settings.skinnedRendererProfile;

            // Ensure SetSettings works as intended.
            settings.enableAnimationCulling = !settings.enableAnimationCulling;
            cullingController.SetSettings(settings);
            settings = cullingController.GetSettings();
            Assert.IsTrue(settings.enableAnimationCulling != prevAnimationCulling, "SetSettings should assign the settings!");
        }

        [Test]
        public void EvaluateRendererVisibility()
        {
            var profile = new CullingControllerProfile();
            profile.emissiveSizeThreshold = 10;
            profile.opaqueSizeThreshold = 20;
            profile.visibleDistanceThreshold = 5;

            // Truth tests
            var farAndBigTest = cullingController.ShouldBeVisible(profile, 30, 20, false, true, true);
            var smallAndNearTest = cullingController.ShouldBeVisible(profile, 5, 2, false, true, true);
            var cameraInBoundsTest = cullingController.ShouldBeVisible(profile, 1, 100, true, true, true);
            var emissiveTest = cullingController.ShouldBeVisible(profile, 15, 20, false, false, true);
            var translucentTest = cullingController.ShouldBeVisible(profile, 1, 50, false, false, false);

            Assert.IsTrue(farAndBigTest);
            Assert.IsTrue(smallAndNearTest);
            Assert.IsTrue(cameraInBoundsTest);
            Assert.IsTrue(emissiveTest);
            Assert.IsTrue(translucentTest);

            // False tests
            var farAndSmallTest = cullingController.ShouldBeVisible(profile, 5, 20, false, true, true);
            var emissiveAndFarTest = cullingController.ShouldBeVisible(profile, 5, 20, false, false, true);

            Assert.IsFalse(farAndSmallTest);
            Assert.IsFalse(emissiveAndFarTest);
        }

        [Test]
        public void EvaluateShadowVisibility()
        {
            var profile = new CullingControllerProfile();
            profile.emissiveSizeThreshold = 10;
            profile.opaqueSizeThreshold = 20;
            profile.visibleDistanceThreshold = 5;

            var farAndSmallTest = cullingController.ShouldHaveShadow(profile, 5, 20, 10);
        }

        [Test]
        public void EvaluateSkinnedMeshesOffscreenUpdate()
        {
            cullingController.
        }

        [Test]
        public void ResetObjects()
        {
        }

        [Test]
        public void ProcessAnimationCulling()
        {
        }

        [Test]
        public void ProcessProfile()
        {
        }
    }
}