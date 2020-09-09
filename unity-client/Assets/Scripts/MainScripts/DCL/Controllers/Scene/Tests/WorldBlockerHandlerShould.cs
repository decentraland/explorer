using DCL.Configuration;
using DCL.Models;
using DCL.Components;
using DCL;
using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class WorldBlockerHandlerShould : TestsBase
    {
        [UnityTest]
        public IEnumerator PutBlockersAroundExplorableArea()
        {
            var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return new WaitForAllMessagesProcessed();
            yield return null;

            sceneController.loadedScenes["xxx"].SetInitMessagesDone();
            yield return null;

            var worldBlockersController = GameObject.FindObjectOfType<WorldBlockersController>();
            Assert.IsNotNull(worldBlockersController);
            yield return null;

            var blockersHandler = Reflection_GetField<WorldBlockerHandler>(worldBlockersController, "blockerHandler");
            var blockers = Reflection_GetField<Dictionary<Vector2Int, PoolableObject>>(blockersHandler, "blockers");

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
    }
}