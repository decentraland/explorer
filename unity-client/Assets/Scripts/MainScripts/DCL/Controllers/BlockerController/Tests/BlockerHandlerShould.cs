using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using DCL;
using System.Linq;
using NUnit.Framework;

namespace Tests
{
    public class FakeBlockerHandler : IBlockerHandler
    {
        public HashSet<Vector2Int> allLoadedParcelCoords = new HashSet<Vector2Int>();

        public void SetupGlobalBlockers(HashSet<Vector2Int> allLoadedParcelCoords, float height, Transform parent)
        {
            this.allLoadedParcelCoords = allLoadedParcelCoords;
        }

        public void CleanBlockers()
        {
            allLoadedParcelCoords.Clear();
        }

        public Dictionary<Vector2Int, PoolableObject> GetBlockers()
        {
            return null;
        }
    }

    public class FakeSceneHandler : ISceneHandler
    {
        public HashSet<Vector2Int> GetAllLoadedScenesCoords()
        {
            var allLoadedParcelCoords = new HashSet<Vector2Int>();
            allLoadedParcelCoords.Add(new Vector2Int(0, 0));

            return allLoadedParcelCoords;
        }
    }

    public class BlockerHandlerShould
    {
        // protected override bool enableSceneIntegrityChecker => false;

        // const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";
        // WorldBlockersController worldBlockersController;
        // BlockerHandler blockersHandler;
        // Dictionary<Vector2Int, PoolableObject> blockers;

        // [UnitySetUp]
        // protected override IEnumerator SetUp()
        // {
        //     yield return base.SetUp();

        //     worldBlockersController = Reflection_GetField<WorldBlockersController>(sceneController, "worldBlockersController");

        //     Assert.IsNotNull(worldBlockersController);

        //     blockersHandler = Reflection_GetField<BlockerHandler>(worldBlockersController, "blockerHandler");
        //     blockers = Reflection_GetField<Dictionary<Vector2Int, PoolableObject>>(blockersHandler, "blockers");

        //     if (!PoolManager.i.ContainsPool(PARCEL_BLOCKER_POOL_NAME))
        //     {
        //         GameObject go = Object.Instantiate(Reflection_GetStaticField<GameObject>(typeof(BlockerHandler), "blockerPrefab"));
        //         Pool pool = PoolManager.i.AddPool(PARCEL_BLOCKER_POOL_NAME, go);
        //         pool.persistent = true;
        //         pool.ForcePrewarm();
        //     }
        // }

        IBlockerHandler blockerHandler;
        GameObject blockersParent;

        [SetUp]
        protected void SetUp()
        {
            blockerHandler = new BlockerHandler(new DCLCharacterPosition());
            blockersParent = new GameObject();
        }

        [TearDown]
        protected void TearDown()
        {
            Object.Destroy(blockersParent);
        }

        [Test]
        public void BlockerHandlerTest()
        {
            var allLoadedParcelCoords = new HashSet<Vector2Int>();
            allLoadedParcelCoords.Add(new Vector2Int(0, 0));

            blockerHandler.SetupGlobalBlockers(allLoadedParcelCoords, 10, blockersParent.transform);

            var blockers = blockerHandler.GetBlockers();

            Assert.AreEqual(blockers.Count(), 8);
            Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 0)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, -1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));
            Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, -1)));
        }

        // TODO: Add test that checks diff blocks ramove/add
    }

    public class BlockersControllerShould
    {
        WorldBlockersController blockerController;
        FakeBlockerHandler blockerHandler;
        GameObject blockersParent;

        [SetUp]
        protected void SetUp()
        {
            blockerHandler = new FakeBlockerHandler();
            blockersParent = new GameObject();
            blockerController = new WorldBlockersController(new FakeSceneHandler(), blockerHandler, new DCLCharacterPosition());
        }

        [TearDown]
        protected void TearDown()
        {
            Object.Destroy(blockersParent);
        }

        [Test]
        public void WorldBlockerControllerBlockers()
        {
            blockerController.SetupWorldBlockers();
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 1);

            blockerController.SetEnabled(false);
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 0);

            blockerController.SetupWorldBlockers();
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 0);

            blockerController.SetEnabled(true);
            blockerController.SetupWorldBlockers();
            Assert.IsTrue(blockerHandler.allLoadedParcelCoords.Count == 1);
        }
    }
}







        // [UnityTest]
        // public IEnumerator PutBlockersAroundExplorableArea()
        // {
        //     var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        //     sceneController.LoadParcelScenes(jsonMessageToLoad);

        //     yield return new WaitForAllMessagesProcessed();
        //     yield return null;

        //     sceneController.loadedScenes["xxx"].SetInitMessagesDone();
        //     yield return null;

        //     Assert.AreEqual(blockers.Count(), 12);

        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 0)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, -1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, 1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, -1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(1, -1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 0)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, -1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 1)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-1, 2)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 2)));
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(-2, 2)));
        // }

        // [UnityTest]
        // public IEnumerator ClearOnlyChangedBlockers()
        // {
        //     // Load first scene
        //     var firstSceneJson = "{\"id\":\"firstScene\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        //     sceneController.LoadParcelScenes(firstSceneJson);

        //     yield return new WaitForAllMessagesProcessed();
        //     yield return null;

        //     sceneController.loadedScenes["firstScene"].SetInitMessagesDone();
        //     yield return null;

        //     Assert.AreEqual(blockers.Count(), 12);

        //     // Save instante id of some blockers that shouldn't change on the next scene load
        //     var targetBlocker1InstanceId = blockers[new Vector2Int(-1, -1)].gameObject.GetInstanceID();
        //     var targetBlocker2InstanceId = blockers[new Vector2Int(-2, -1)].gameObject.GetInstanceID();
        //     var targetBlocker3InstanceId = blockers[new Vector2Int(-2, 0)].gameObject.GetInstanceID();

        //     // check blocker that will get removed on next scene load
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));

        //     // Load 2nd scene next to the first one
        //     var secondSceneJson = "{\"id\":\"secondScene\",\"basePosition\":{\"x\":0,\"y\":1},\"parcels\":[{\"x\":0,\"y\":2}, {\"x\":0,\"y\":1}, {\"x\":1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        //     sceneController.LoadParcelScenes(secondSceneJson);

        //     yield return new WaitForAllMessagesProcessed();
        //     yield return null;

        //     sceneController.loadedScenes["secondScene"].SetInitMessagesDone();
        //     yield return null;

        //     Assert.AreEqual(blockers.Count(), 16);

        //     // Check some non-changed blockers:
        //     Assert.IsTrue(blockers[new Vector2Int(-1, -1)].gameObject.GetInstanceID() == targetBlocker1InstanceId);
        //     Assert.IsTrue(blockers[new Vector2Int(-2, -1)].gameObject.GetInstanceID() == targetBlocker2InstanceId);
        //     Assert.IsTrue(blockers[new Vector2Int(-2, 0)].gameObject.GetInstanceID() == targetBlocker3InstanceId);

        //     // Check removed blocker
        //     Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 1)));
        // }

        // [UnityTest]
        // public IEnumerator RemoveBlockersOnNewlyLoadedScene()
        // {
        //     // Load first scene
        //     var firstSceneJson = "{\"id\":\"firstScene\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        //     sceneController.LoadParcelScenes(firstSceneJson);

        //     yield return new WaitForAllMessagesProcessed();
        //     yield return null;

        //     sceneController.loadedScenes["firstScene"].SetInitMessagesDone();
        //     yield return null;

        //     Assert.AreEqual(blockers.Count(), 12);

        //     // Load 2nd scene next to the first one
        //     var secondSceneJson = "{\"id\":\"secondScene\",\"basePosition\":{\"x\":0,\"y\":1},\"parcels\":[{\"x\":0,\"y\":2}, {\"x\":0,\"y\":1}, {\"x\":1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        //     sceneController.LoadParcelScenes(secondSceneJson);

        //     yield return new WaitForAllMessagesProcessed();
        //     yield return null;

        //     // check blocker from previous load is on the new scene that still didn't finish loading
        //     Assert.IsTrue(blockers.ContainsKey(new Vector2Int(0, 1)));

        //     sceneController.loadedScenes["secondScene"].SetInitMessagesDone();
        //     yield return null;

        //     Assert.AreEqual(blockers.Count(), 16);

        //     // Check the blocker was removed
        //     Assert.IsFalse(blockers.ContainsKey(new Vector2Int(0, 1)));
        // }

        // [UnityTest]
        // public IEnumerator NotInstantiateBlockersInDebugMode()
        // {
        //     SceneController.i.SetDebug();
        //     DCL.Configuration.EnvironmentSettings.DEBUG = true;

        //     yield return null;

        //     var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        //     sceneController.LoadParcelScenes(jsonMessageToLoad);

        //     yield return new WaitForAllMessagesProcessed();
        //     yield return null;

        //     sceneController.loadedScenes["xxx"].SetInitMessagesDone();
        //     yield return null;

        //     Assert.AreEqual(blockers.Count(), 0);

        //     sceneController.isDebugMode = false;
        // }