﻿using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

namespace Tests
{
    public class LoadableShapesTests : TestsBase
    {
        [UnityTest]
        public IEnumerator OBJShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Material placeholderLoadingMaterial = Resources.Load<Material>("Materials/AssetLoading");

            yield return null;

            Assert.IsTrue(scene.entities[entityId].meshRootGameObject == null,
                "Since the shape hasn't been updated yet, the child mesh shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/OBJ/teapot.obj"
                }));

            LoadWrapper_OBJ objShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_OBJ>(true);
            yield return new WaitUntil(() => objShape.alreadyLoaded);

            Assert.IsTrue(scene.entities[entityId].meshRootGameObject != null,
                "Every entity with a shape should have the mandatory 'Mesh' object as a child");

            var childRenderer = scene.entities[entityId].meshRootGameObject.GetComponentInChildren<MeshRenderer>();
            Assert.IsTrue(childRenderer != null,
                "Since the shape has already been updated, the child renderer should exist");
            Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial,
                "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
        }

        [UnityTest]
        public IEnumerator GLTFShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
        }

        [UnityTest]
        public IEnumerator NFTShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            var entity = scene.entities[entityId];
            Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestHelpers.SharedComponentAttach(component, entity, "");

            Assert.IsTrue(entity.meshRootGameObject != null, "entity mesh object should already exist as the NFTShape already initialized");

            var nftShape = entity.meshRootGameObject.GetComponent<LoadWrapper_NFT>();
            var backgroundMaterialPropertyBlock = new MaterialPropertyBlock();
            nftShape.loaderController.meshRenderer.GetPropertyBlock(backgroundMaterialPropertyBlock, 1);

            Assert.IsTrue(backgroundMaterialPropertyBlock.GetColor("_BaseColor") == new Color(0.6404918f, 0.611472f, 0.8584906f), "The NFT frame background color should be the default one");

            // Update color and check if it changed
            componentModel.color = Color.yellow;
            yield return TestHelpers.SharedComponentUpdate(component, componentModel);

            nftShape.loaderController.meshRenderer.GetPropertyBlock(backgroundMaterialPropertyBlock, 1);
            Assert.AreEqual(Color.yellow, backgroundMaterialPropertyBlock.GetColor("_BaseColor"), "The NFT frame background color should be yellow");
        }

        [UnityTest]
        public IEnumerator NFTShapeMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            var component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE);
            yield return component.routine;

            Assert.IsFalse(component == null);

            yield return TestHelpers.TestSharedComponentDefaultsOnUpdate<NFTShape.Model, NFTShape>(scene, CLASS_ID.NFT_SHAPE);
        }

        [UnityTest]
        public IEnumerator PreExistentShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            Assert.IsTrue(entity.meshRootGameObject == null, "meshGameObject should be null");

            // Set its shape as a BOX
            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");
            yield return scene.GetSharedComponent(componentId).routine;

            var meshName = entity.meshRootGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Box Instance", meshName);

            // Update its shape to a cylinder
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");
            yield return scene.GetSharedComponent(componentId).routine;

            Assert.IsTrue(entity.meshRootGameObject != null, "meshGameObject should not be null");

            meshName = entity.meshRootGameObject.GetComponent<MeshFilter>().mesh.name;
            Assert.AreEqual("DCL Cylinder Instance", meshName);
            Assert.IsTrue(entity.meshRootGameObject.GetComponent<MeshFilter>() != null,
                "After updating the entity shape to a basic shape, the mesh filter shouldn't be removed from the object");

            Assert.IsTrue(entity.meshesInfo.currentShape != null, "current shape must exist 1");
            Assert.IsTrue(entity.meshesInfo.currentShape is CylinderShape, "current shape is BoxShape");

            // Update its shape to a GLTF
            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.IsTrue(entity.meshesInfo.currentShape != null, "current shape must exist 2");
            Assert.IsTrue(entity.meshesInfo.currentShape is GLTFShape, "current shape is GLTFShape");

            Assert.IsTrue(entity.meshRootGameObject != null);

            Assert.IsTrue(entity.meshRootGameObject.GetComponent<MeshFilter>() == null,
                "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            // Update its shape to a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.SPHERE_SHAPE, "{}");
            yield return scene.GetSharedComponent(componentId).routine;

            yield return null;

            Assert.IsTrue(entity.meshRootGameObject != null);

            meshName = entity.meshRootGameObject.GetComponent<MeshFilter>().mesh.name;

            Assert.AreEqual("DCL Sphere Instance", meshName);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            {
                var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

                Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in first object!");
                Assert.IsTrue(gltfObject.transform.Find("Lantern") != null, "Can't find \"Lantern!\"");
            }

            TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
                }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            {
                var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

                Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in second object!");
                Assert.IsTrue(gltfObject.transform.Find("node_damagedHelmet_-6514") != null,
                    "Can't find \"node_damagedHelmet_-6514\"!");
            }
        }

        [UnityTest]
        public IEnumerator PreExistentGLTFShapeImmediateUpdate()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
                new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
                }));

            gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            Assert.AreEqual(1,
                scene.entities[entityId].gameObject.GetComponentsInChildren<InstantiatedGLTFObject>().Length,
                "Only 1 'InstantiatedGLTFObject' should remain once the GLTF shape has been updated");
        }

        [UnityTest]
        public IEnumerator GLTFShapeAttachedGetsReplacedOnNewAttachment()
        {
            yield return InitScene();

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            // set first GLTF
            string gltfId1 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
                }));
            var gltf1 = scene.GetSharedComponent(gltfId1);

            LoadWrapper_GLTF gltfLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            Assert.AreEqual(gltf1, entity.GetSharedComponent(typeof(BaseShape)));

            // set second GLTF
            string gltfId2 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            gltfLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            Assert.AreEqual(scene.GetSharedComponent(gltfId2), entity.GetSharedComponent(typeof(BaseShape)));
            Assert.IsFalse(gltf1.attachedEntities.Contains(entity));
        }

        // NOTE: Since every deployed scene carries compiled core code of the sdk at the moment of deploying, most of them have GLTFs default 'withCollisions' value
        // different than the new default value, that's why we are forcing the colliders to be ON on EVERY GLTF until that gets fixed, and that's why this test fails.
        [UnityTest]
        [Explicit]
        public IEnumerator GLTFShapeCollisionProperty()
        {
            yield return InitScene();

            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            yield return null;

            // Create shape component
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb";

            var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity, "");

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);
        }

        [UnityTest]
        public IEnumerator GLTFVisibleProperty()
        {
            yield return InitScene();

            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            yield return null;

            // Create shape component
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = Utils.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb";

            var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity, "");

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);
        }

        [UnityTest]
        public IEnumerator NFTShapeCollisionProperty()
        {
            yield return InitScene();

            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            yield return null;

            // Create shape component
            var shapeModel = new LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model();
            shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

            var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_NFT, NFTShape.Model>, LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity, "");

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);
        }

        [UnityTest]
        public IEnumerator NFTShapeVisibleProperty()
        {
            yield return InitScene();

            string entityId = "entityId";
            TestHelpers.CreateSceneEntity(scene, entityId);
            var entity = scene.entities[entityId];

            TestHelpers.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            yield return null;

            // Create shape component
            var shapeModel = new LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model();
            shapeModel.src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536";

            var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_NFT, NFTShape.Model>, LoadableShape<LoadWrapper_NFT, NFTShape.Model>.Model>(scene, CLASS_ID.NFT_SHAPE, shapeModel);
            yield return shapeComponent.routine;

            TestHelpers.SharedComponentAttach(shapeComponent, entity, "");

            var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_NFT>(true);
            yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

            yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);
        }

        [UnityTest]
        public IEnumerator GLTF_OnReadyBeforeLoading()
        {
            yield return InitScene();
            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, Utils.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);

            bool isOnReady = false;
            gltfShape.CallWhenReady((x) => { isOnReady = true; });

            Assert.IsFalse(isOnReady);
        }

        [UnityTest]
        public IEnumerator GLTF_OnReadyWaitLoading()
        {
            yield return InitScene();
            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, Utils.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);

            bool isOnReady = false;
            gltfShape.CallWhenReady((x) => { isOnReady = true; });
            yield return TestHelpers.WaitForGLTFLoad(entity);

            Assert.IsTrue(isOnReady);
        }

        [UnityTest]
        public IEnumerator GLTF_OnReadyWithoutAttachInstantlyCalled()
        {
            yield return InitScene();

            GLTFShape gltfShape = TestHelpers.SharedComponentCreate<GLTFShape, GLTFShape.Model>(scene, CLASS_ID.GLTF_SHAPE, new LoadableShape.Model()
            {
                src = Utils.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb"
            });

            bool isOnReady = false;
            gltfShape.CallWhenReady((x) => { isOnReady = true; });

            Assert.IsTrue(isOnReady);
        }

        [UnityTest]
        public IEnumerator GLTF_OnReadyAfterLoadingInstantlyCalled()
        {
            yield return InitScene();
            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, Utils.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);
            yield return TestHelpers.WaitForGLTFLoad(entity);

            bool isOnReady = false;
            gltfShape.CallWhenReady((x) => { isOnReady = true; });
            Assert.IsTrue(isOnReady);
        }
    }
}
