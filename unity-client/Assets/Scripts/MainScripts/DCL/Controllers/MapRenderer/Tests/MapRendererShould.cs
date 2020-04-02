using DCL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class MapRendererShould : TestsBase
    {
        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return InitUnityScene("MainTest");

            if (MapRenderer.i == null)
                Object.Instantiate(Resources.Load("Map Renderer"));

            MapRenderer.i.atlas.mapChunkPrefab = (GameObject)Resources.Load("Map Chunk Mock");
            var go = new GameObject("Viewport");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * 100;
            MapRenderer.i.atlas.viewport = rt;

            yield return null;
        }

        [UnityTest]
        public IEnumerator PositionPlayerIconAsIntended()
        {
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(0, 0));
            Assert.AreApproximatelyEqual(2990, MapRenderer.i.playerPositionIcon.transform.localPosition.x);
            Assert.AreApproximatelyEqual(2990, MapRenderer.i.playerPositionIcon.transform.localPosition.y);

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(500, 0, 500));
            Assert.AreApproximatelyEqual(3615, MapRenderer.i.playerPositionIcon.transform.localPosition.x);
            Assert.AreApproximatelyEqual(3615, MapRenderer.i.playerPositionIcon.transform.localPosition.y);

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(-500, 0, -500));
            Assert.AreApproximatelyEqual(2365, MapRenderer.i.playerPositionIcon.transform.localPosition.x);
            Assert.AreApproximatelyEqual(2365, MapRenderer.i.playerPositionIcon.transform.localPosition.y);

            CommonScriptableObjects.cameraForward.Set(new Vector3(0, 0, 1));
            Assert.AreApproximatelyEqual(0, MapRenderer.i.playerPositionIcon.transform.eulerAngles.z);
            CommonScriptableObjects.cameraForward.Set(new Vector3(0, 0, -1));
            Assert.AreApproximatelyEqual(180, MapRenderer.i.playerPositionIcon.transform.eulerAngles.z);
            CommonScriptableObjects.cameraForward.Set(new Vector3(0.5f, 0, 0.5f));
            Assert.AreApproximatelyEqual(315, MapRenderer.i.playerPositionIcon.transform.eulerAngles.z);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CenterAsIntended()
        {
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(0, 0, 0));
            Assert.AreApproximatelyEqual(-1495, MapRenderer.i.atlas.container.transform.position.x);
            Assert.AreApproximatelyEqual(-1495, MapRenderer.i.atlas.container.transform.position.y);

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(100, 0, 100));
            Assert.AreApproximatelyEqual(-1557.5f, MapRenderer.i.atlas.container.transform.position.x);
            Assert.AreApproximatelyEqual(-1557.5f, MapRenderer.i.atlas.container.transform.position.y);

            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(-100, 0, -100));
            Assert.AreApproximatelyEqual(-1432.5f, MapRenderer.i.atlas.container.transform.position.x);
            Assert.AreApproximatelyEqual(-1432.5f, MapRenderer.i.atlas.container.transform.position.y);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerformCullingAsIntended()
        {
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(0, 0));
            Assert.AreEqual("1111111111111111111111110011111001111111111111111", GetChunkStatesAsString());
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(1000, 0, 1000));
            Assert.AreEqual("1111111111111111111111111111111100111110011111111", GetChunkStatesAsString());
            CommonScriptableObjects.playerWorldPosition.Set(new Vector3(-1000, 0, -1000));
            Assert.AreEqual("1111111111111111001111100111111111111111111111111", GetChunkStatesAsString());
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisplayParcelOfInterestIconsProperly()
        {
            var sceneInfo = new MinimapMetadata.MinimapSceneInfo();
            sceneInfo.name = "important scene";
            sceneInfo.isPOI = true;
            sceneInfo.parcels = new List<Vector2Int>()
            {
                new Vector2Int() { x = 0, y = 0 },
                new Vector2Int() { x = 0, y = 1 },
                new Vector2Int() { x = 1, y = 0 },
                new Vector2Int() { x = 1, y = 1 }
            };

            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo);

            var sceneInfo2 = new MinimapMetadata.MinimapSceneInfo();
            sceneInfo2.name = "non-important scene";
            sceneInfo2.isPOI = false;
            sceneInfo2.parcels = new List<Vector2Int>()
            {
                new Vector2Int() { x = 5, y = 0 },
            };

            MinimapMetadata.GetMetadata().AddSceneInfo(sceneInfo2);

            MapSceneIcon[] icons = MapRenderer.i.GetComponentsInChildren<MapSceneIcon>();

            Assert.AreEqual(1, icons.Length, "Only 1 icon is marked as POI, but 2 icons were spawned");
            Assert.AreEqual(sceneInfo.name, icons[0].title.text);
            Assert.AreEqual(new Vector3(3000, 3000, 0), icons[0].transform.localPosition);

            yield return null;
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
