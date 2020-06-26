using DCL;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests
{
    public class MapRendererShould : TestsBase
    {
        protected override bool justSceneSetUp => true;

        private GameObject viewport;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            if (MapRenderer.i == null)
                Object.Instantiate(Resources.Load("Map Renderer"));

            MapRenderer.i.atlas.mapChunkPrefab = (GameObject) Resources.Load("Map Chunk Mock");
            viewport = new GameObject("Viewport");
            var rt = viewport.AddComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * 100;
            MapRenderer.i.atlas.viewport = rt;
        }

        protected override IEnumerator TearDown()
        {
            MapRenderer.i.Cleanup();
            UnityEngine.Object.Destroy(viewport);

            yield return base.TearDown();
        }

        [Test]
        public void CenterAsIntended()
        {
            Transform atlasContainerTransform = MapRenderer.i.atlas.container.transform;

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(0, 0, 0));
            Assert.AreApproximatelyEqual(-1500, atlasContainerTransform.position.x);
            Assert.AreApproximatelyEqual(-1500, atlasContainerTransform.position.y);

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(100, 0, 100));
            Assert.AreApproximatelyEqual(-1562.5f, atlasContainerTransform.position.x);
            Assert.AreApproximatelyEqual(-1562.5f, atlasContainerTransform.position.y);

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(-100, 0, -100));
            Assert.AreApproximatelyEqual(-1437.5f, atlasContainerTransform.position.x);
            Assert.AreApproximatelyEqual(-1437.5f, atlasContainerTransform.position.y);
        }

        [Test]
        public void PerformCullingAsIntended()
        {
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(0, 0));
            Assert.AreEqual("1111111111111111111111110111111111111111111111111", GetChunkStatesAsString());
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(1000, 0, 1000));
            Assert.AreEqual("1111111111111111111111111111111100111110011111111", GetChunkStatesAsString());
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(-1000, 0, -1000));
            Assert.AreEqual("1111111100111110011111111111111111111111111111111", GetChunkStatesAsString());
        }

        [Test]
        public void DisplayParcelOfInterestIconsProperly()
        {
            var sceneInfo = new MinimapMetadata.MinimapSceneInfo();
            sceneInfo.name = "important scene";
            sceneInfo.isPOI = true;
            sceneInfo.parcels = new List<Vector2Int>()
            {
                new Vector2Int() {x = 0, y = 0},
                new Vector2Int() {x = 0, y = 1},
                new Vector2Int() {x = 1, y = 0},
                new Vector2Int() {x = 1, y = 1}
            };

            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

            var sceneInfo2 = new MinimapMetadata.MinimapSceneInfo();
            sceneInfo2.name = "non-important scene";
            sceneInfo2.isPOI = false;
            sceneInfo2.parcels = new List<Vector2Int>()
            {
                new Vector2Int() {x = 5, y = 0},
            };

            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo2);

            MapSceneIcon[] icons = MapRenderer.i.GetComponentsInChildren<MapSceneIcon>();

            Assert.AreEqual(1, icons.Length, "Only 1 icon is marked as POI, but 2 icons were spawned");
            Assert.AreEqual(sceneInfo.name, icons[0].title.text);
            Assert.AreEqual(new Vector3(3010, 3010, 0), icons[0].transform.localPosition);
        }

        [Test]
        public void DisplayAndUpdateUserIconProperly()
        {
            Vector3 initialPosition = new Vector3(100, 0, 50);
            Vector3 modifiedPosition = new Vector3(150, 0, -30);

            var userInfo = new MinimapMetadata.MinimapUserInfo();
            userInfo.userId = "testuser";
            userInfo.worldPosition = initialPosition;

            // Create an user icon
            MinimapMetadata.GetMetadata().AddOrUpdateUserInfo(userInfo);

            MapSceneIcon[] icons = MapRenderer.i.GetComponentsInChildren<MapSceneIcon>();

            Assert.AreEqual(1, icons.Length, "There should be only 1 user icon");
            Vector2 iconGridPosition = DCL.Helpers.Utils.WorldToGridPositionUnclamped(initialPosition);
            Assert.AreEqual(DCL.Helpers.MapUtils.GetTileToLocalPosition(iconGridPosition.x, iconGridPosition.y), icons[0].transform.localPosition);

            // Modifify the position of the user icon
            userInfo.worldPosition = modifiedPosition;

            MinimapMetadata.GetMetadata().AddOrUpdateUserInfo(userInfo);

            icons = MapRenderer.i.GetComponentsInChildren<MapSceneIcon>();

            Assert.AreEqual(1, icons.Length, "There should still be the same user icon");
            iconGridPosition = DCL.Helpers.Utils.WorldToGridPositionUnclamped(modifiedPosition);
            Assert.AreEqual(DCL.Helpers.MapUtils.GetTileToLocalPosition(iconGridPosition.x, iconGridPosition.y), icons[0].transform.localPosition);

            // Remove the user icon
            MinimapMetadata.GetMetadata().RemoveUserInfo(userInfo.userId);

            icons = MapRenderer.i.GetComponentsInChildren<MapSceneIcon>();

            Assert.AreEqual(0, icons.Length, "There should not be any user icon");
        }

        public string GetChunkStatesAsString()
        {
            string result = "";
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    MapChunk chunk = MapRenderer.i.atlas.GetChunk(x, y);

                    if (chunk == null)
                        result += "-";
                    else if (chunk.targetImage.enabled)
                        result += "0";
                    else
                        result += "1";
                }
            }

            return result;
        }
    }
}