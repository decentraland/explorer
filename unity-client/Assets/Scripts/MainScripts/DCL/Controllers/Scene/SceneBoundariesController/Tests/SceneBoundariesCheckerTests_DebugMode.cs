using NUnit.Framework;
using System.Collections;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests_DebugMode : TestsBase
    {
        protected override bool enableSceneIntegrityChecker => false;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return SetUp_SceneController(debugMode: true);
            yield return SetUp_CharacterController();

            SetUp_Renderer();

            sceneController.boundariesChecker.timeBetweenChecks = 0f;

            UnityEngine.Assertions.Assert.IsTrue(sceneController.useBoundariesChecker);
            UnityEngine.Assertions.Assert.IsTrue(sceneController.boundariesChecker is SceneBoundariesDebugModeChecker);
        }

        [UnityTest]
        public IEnumerator ResetMaterialCorrectlyWhenInvalidEntitiesAreRemoved()
        {
            sceneController.isDebugMode = true;

            var entity = TestHelpers.CreateSceneEntity(scene);
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(8, 1, 8)});
            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            yield return null;

            Assert.IsFalse(SBC_Asserts.MeshIsInvalid(entity.meshesInfo));
            // Move object to surpass the scene boundaries
            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model {position = new Vector3(18, 1, 18)});

            yield return null;

            Assert.IsTrue(SBC_Asserts.MeshIsInvalid(entity.meshesInfo));

            TestHelpers.RemoveSceneEntity(scene, entity.entityId);

            ParcelScene.parcelScenesCleaner.ForceCleanup();

            yield return null;

            var entity2 = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity2, new DCLTransform.Model {position = new Vector3(8, 1, 8)});
            TestHelpers.CreateAndSetShape(scene, entity2.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));

            LoadWrapper gltfShape2 = GLTFShape.GetLoaderForEntity(entity2);

            yield return new WaitUntil(() => gltfShape2.alreadyLoaded);
            yield return null;

            Assert.IsFalse(SBC_Asserts.MeshIsInvalid(entity2.meshesInfo));
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsResetWhenReenteringBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene);
            sceneController.isDebugMode = false;
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);
            yield return SBC_Asserts.ChildShapeIsEvaluated(scene);
        }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParentDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene);
        }

        [UnityTest]
        public IEnumerator HeightIsEvaluatedDebugMode()
        {
            yield return InitScene(false, true, true, true, debugMode: true);

            yield return SBC_Asserts.HeightIsEvaluated(scene);
        }

        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBoundsDebugMode()
        {
            sceneController.isDebugMode = true;
            yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene);
            sceneController.isDebugMode = false;
        }
    }
}