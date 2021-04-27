using System.Collections;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;
using WaitUntil = UnityEngine.WaitUntil;

namespace Tests
{
    public class UUIDComponentShould : IntegrationTestSuite
    {
        private ParcelScene scene;

        protected override WorldRuntimeContext CreateRuntimeContext()
        {
            return DCL.Tests.WorldRuntimeContextFactory.CreateWithCustomMocks
            (
                sceneController: new SceneController(),
                state: new WorldState(),
                componentFactory: new RuntimeComponentFactory()
            );
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = Environment.i.world.sceneController.CreateTestScene() as ParcelScene;
        }

        [UnityTest]
        public IEnumerator BeDestroyedCorrectlyWhenReceivingComponentDestroyMessage()
        {
            var shape = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            IDCLEntity entity = shape.attachedEntities.First();

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var model = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };

            TestHelpers.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            model.type = OnPointerDown.NAME;

            TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            model.type = OnPointerUp.NAME;

            TestHelpers.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_CLICK ));
            Assert.IsTrue( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_UP ));
            Assert.IsTrue( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_DOWN ));

            scene.EntityComponentRemove(entity.entityId, OnPointerDown.NAME);
            scene.EntityComponentRemove(entity.entityId, OnPointerUp.NAME);
            scene.EntityComponentRemove(entity.entityId, OnClick.NAME);

            Assert.IsFalse( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_CLICK ));
            Assert.IsFalse( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_UP ));
            Assert.IsFalse( entity.components.ContainsKey( CLASS_ID_COMPONENT.UUID_ON_DOWN ));
        }
        
        [UnityTest]
        public IEnumerator NotCreateCollidersOnLoadingShape()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            string entityId = "1";
            var entity = TestHelpers.CreateSceneEntity(scene, entityId);
            
            string onPointerId = "pointerevent-1";
            var model = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var onPointerDownComponent = TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);
            
            // 2. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(shapeModel));
            
            // 4. Change a shape component property while it loads
            shapeModel.visible = false;
            TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(shapeModel));

            var pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders; 
            Assert.IsTrue(pointerEventColliders == null || pointerEventColliders.Length == 0);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);
            
            // 5. Check if the PointerEvent colliders were created
            pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders; 
            Assert.IsTrue(pointerEventColliders != null && pointerEventColliders.Length > 0);
        }
        
        [UnityTest]
        public IEnumerator NotRecreateCollidersWhenShapeDoesntChange()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            string entityId = "1";
            var entity = TestHelpers.CreateSceneEntity(scene, entityId);
            
            string onPointerId = "pointerevent-1";
            var model = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var onPointerDownComponent = TestHelpers.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);
            
            // 2. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(shapeModel));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);
            
            var pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders; 
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);
            
            // 3. Change a shape component property conserving the same glb
            shapeModel.visible = false;
            TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(shapeModel));
            yield return null;
            
            // 4. Check the same colliders were kept 
            Assert.IsTrue(pointerEventColliders == onPointerDownComponent.pointerEventHandler.eventColliders.colliders);
        }
    }
}