using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
using DCL;
using System.Linq;

namespace Tests
{
    public class BlockerHandlerShould : TestsBase
    {
        protected override bool enableSceneIntegrityChecker => false;

        const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";
        WorldBlockersController worldBlockersController;
        BlockerHandler blockersHandler;
        Dictionary<Vector2Int, PoolableObject> blockers;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            worldBlockersController = Reflection_GetField<WorldBlockersController>(sceneController, "worldBlockersController");

            Assert.IsNotNull(worldBlockersController);

            blockersHandler = Reflection_GetField<BlockerHandler>(worldBlockersController, "blockerHandler");
            blockers = Reflection_GetField<Dictionary<Vector2Int, PoolableObject>>(blockersHandler, "blockers");

            if (!PoolManager.i.ContainsPool(PARCEL_BLOCKER_POOL_NAME))
            {
                GameObject go = Object.Instantiate(Reflection_GetStaticField<GameObject>(typeof(BlockerHandler), "blockerPrefab"));
                Pool pool = PoolManager.i.AddPool(PARCEL_BLOCKER_POOL_NAME, go);
                pool.persistent = true;
                pool.ForcePrewarm();
            }
        }

        [UnityTest]
        public IEnumerator PutBlockersAroundExplorableArea()
        {
            var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            sceneController.loadedScenes["xxx"].SetInitMessagesDone();
            yield return null;

            Assert.AreEqual(blockers.Count(), 12);

            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, 2)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 2)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 2)));
        }

        [UnityTest]
        public IEnumerator ClearOnlyChangedBlockers()
        {
            // Load first scene
            var firstSceneJson = "{\"id\":\"firstScene\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(firstSceneJson);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            sceneController.loadedScenes["firstScene"].SetInitMessagesDone();
            yield return null;

            Assert.AreEqual(blockers.Count(), 12);

            // Save instante id of some blockers that shouldn't change on the next scene load
            var targetBlocker1InstanceId = blockers[new Vector2Int(-1, -1)].gameObject.GetInstanceID();
            var targetBlocker2InstanceId = blockers[new Vector2Int(-2, -1)].gameObject.GetInstanceID();
            var targetBlocker3InstanceId = blockers[new Vector2Int(-2, 0)].gameObject.GetInstanceID();

            // check blocker that will get removed on next scene load
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));

            // Load 2nd scene next to the first one
            var secondSceneJson = "{\"id\":\"secondScene\",\"basePosition\":{\"x\":0,\"y\":1},\"parcels\":[{\"x\":0,\"y\":2}, {\"x\":0,\"y\":1}, {\"x\":1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(secondSceneJson);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            sceneController.loadedScenes["secondScene"].SetInitMessagesDone();
            yield return null;

            Assert.AreEqual(blockers.Count(), 16);

            // Check some non-changed blockers:
            Assert.IsTrue(blockers[new Vector2Int(-1, -1)].gameObject.GetInstanceID() == targetBlocker1InstanceId);
            Assert.IsTrue(blockers[new Vector2Int(-2, -1)].gameObject.GetInstanceID() == targetBlocker2InstanceId);
            Assert.IsTrue(blockers[new Vector2Int(-2, 0)].gameObject.GetInstanceID() == targetBlocker3InstanceId);

            // Check removed blocker
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 1)));
        }

        [UnityTest]
        public IEnumerator RemoveBlockersOnNewlyLoadedScene()
        {
            // Load first scene
            var firstSceneJson = "{\"id\":\"firstScene\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(firstSceneJson);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            sceneController.loadedScenes["firstScene"].SetInitMessagesDone();
            yield return null;

            Assert.AreEqual(blockers.Count(), 12);

            // Load 2nd scene next to the first one
            var secondSceneJson = "{\"id\":\"secondScene\",\"basePosition\":{\"x\":0,\"y\":1},\"parcels\":[{\"x\":0,\"y\":2}, {\"x\":0,\"y\":1}, {\"x\":1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(secondSceneJson);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            // check blocker from previous load is on the new scene that still didn't finish loading
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));

            sceneController.loadedScenes["secondScene"].SetInitMessagesDone();
            yield return null;

            Assert.AreEqual(blockers.Count(), 16);

            // Check the blocker was removed
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 1)));
        }

        [UnityTest]
        public IEnumerator NotInstantiateBlockersInDebugMode()
        {
            SceneController.i.SetDebug();
            DCL.Configuration.EnvironmentSettings.DEBUG = true;

            yield return null;

            var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            sceneController.loadedScenes["xxx"].SetInitMessagesDone();
            yield return null;

            Assert.AreEqual(blockers.Count(), 0);

            sceneController.isDebugMode = false;
        }
    }
}