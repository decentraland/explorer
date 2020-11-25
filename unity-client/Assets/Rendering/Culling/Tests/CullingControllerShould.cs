using System.Collections;
using System.Collections.Generic;
using DCL.Rendering;
using NSubstitute;
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
            cullingController = CullingController.Create();
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
            var settings = cullingController.GetSettingsCopy();
            Assert.IsNotNull(settings, "Settings should never be null!");

            var prevAnimationCulling = settings.enableAnimationCulling;

            // This is needed because SetSettings sets dirty flag and other stuff. We don't want direct access.
            // Settings is not a property because to make the SetSetting performance hit more obvious.
            settings.enableAnimationCulling = !settings.enableAnimationCulling;
            settings = cullingController.GetSettingsCopy();
            Assert.IsTrue(settings.enableAnimationCulling == prevAnimationCulling, "GetSettings should return a copy!");
        }

        [Test]
        public void SetSettingsProperly()
        {
            // Ensure settings never return null
            var settings = cullingController.GetSettingsCopy();
            Assert.IsNotNull(settings, "Settings should never be null!");

            var prevAnimationCulling = settings.enableAnimationCulling;

            // Ensure SetSettings works as intended.
            settings.enableAnimationCulling = !settings.enableAnimationCulling;
            cullingController.SetSettings(settings);
            settings = cullingController.GetSettingsCopy();
            Assert.IsTrue(settings.enableAnimationCulling != prevAnimationCulling, "SetSettings should assign the settings!");
        }

        [Test]
        public void EvaluateRendererVisibility()
        {
            //Arrange
            var profile = new CullingControllerProfile();
            profile.emissiveSizeThreshold = 10;
            profile.opaqueSizeThreshold = 20;
            profile.visibleDistanceThreshold = 5;

            // Act
            // Truth tests
            var farAndBigTest = CullingControllerUtils.TestRendererVisibleRule(profile, 30, 20, false, true, true);
            var smallAndNearTest = CullingControllerUtils.TestRendererVisibleRule(profile, 5, 2, false, true, true);
            var cameraInBoundsTest = CullingControllerUtils.TestRendererVisibleRule(profile, 1, 100, true, true, true);
            var emissiveTest = CullingControllerUtils.TestRendererVisibleRule(profile, 15, 20, false, false, true);

            // Falsehood tests
            var farAndSmallTest = CullingControllerUtils.TestRendererVisibleRule(profile, 5, 20, false, true, true);
            var emissiveAndFarTest = CullingControllerUtils.TestRendererVisibleRule(profile, 5, 20, false, false, true);
            var farAndTransparentTest = CullingControllerUtils.TestRendererVisibleRule(profile, 1, 50, false, false, false);

            // Assert
            Assert.IsTrue(farAndBigTest);
            Assert.IsTrue(smallAndNearTest);
            Assert.IsTrue(cameraInBoundsTest);
            Assert.IsTrue(emissiveTest);

            Assert.IsFalse(farAndSmallTest);
            Assert.IsFalse(emissiveAndFarTest);
            Assert.IsFalse(farAndTransparentTest);
        }

        [Test]
        public void EvaluateShadowVisibility()
        {
            // Arrange
            var profile = new CullingControllerProfile();
            profile.shadowMapProjectionSizeThreshold = 6;
            profile.shadowRendererSizeThreshold = 20;
            profile.shadowDistanceThreshold = 15;

            // Act
            var nearTest = CullingControllerUtils.TestRendererShadowRule(profile, 1, 5, 10);
            var nearButSmallTexel = CullingControllerUtils.TestRendererShadowRule(profile, 1, 5, 1);
            var farAndBigEnough = CullingControllerUtils.TestRendererShadowRule(profile, 30, 30, 30);
            var farAndSmall = CullingControllerUtils.TestRendererShadowRule(profile, 10, 30, 30);
            var farAndSmallTexel = CullingControllerUtils.TestRendererShadowRule(profile, 10, 30, 1);

            // Assert
            Assert.IsTrue(nearTest);
            Assert.IsTrue(farAndBigEnough);
            Assert.IsFalse(nearButSmallTexel);
            Assert.IsFalse(farAndSmall);
            Assert.IsFalse(farAndSmallTexel);
        }

        [Test]
        public void EvaluateSkinnedMeshesOffscreenUpdate()
        {
        }

        [Test]
        public void ResetObjects()
        {
            // Arrange
            GameObject go1 = new GameObject();
            GameObject go2 = new GameObject();

            var r = go1.AddComponent<MeshRenderer>();
            var skr = go2.AddComponent<SkinnedMeshRenderer>();
            var anim = go2.AddComponent<Animation>();

            r.forceRenderingOff = true;
            skr.updateWhenOffscreen = false;
            anim.cullingType = AnimationCullingType.BasedOnRenderers;

            var mockTracker = Substitute.For<ICullingObjectsTracker>();
            cullingController.objectsTracker = mockTracker;

            mockTracker.GetRenderers().Returns(info => go1.GetComponentsInChildren<Renderer>());
            mockTracker.GetSkinnedRenderers().Returns(info => go2.GetComponentsInChildren<SkinnedMeshRenderer>());
            mockTracker.GetAnimations().Returns(info => go2.GetComponentsInChildren<Animation>());

            // Act
            cullingController.ResetObjects();

            // Assert
            Assert.IsFalse(r.forceRenderingOff);
            Assert.IsTrue(skr.updateWhenOffscreen);
            Assert.IsTrue(anim.cullingType == AnimationCullingType.AlwaysAnimate);

            // Annihilate
            Object.Destroy(go1);
            Object.Destroy(go2);
        }

        [UnityTest]
        public IEnumerator ProcessAnimationCulling()
        {
            yield break;
        }

        [UnityTest]
        public IEnumerator ProcessProfile()
        {
            yield break;
        }
    }
}