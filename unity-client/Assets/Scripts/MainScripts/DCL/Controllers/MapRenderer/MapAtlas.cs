using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MapAtlas : MonoBehaviour
    {
        public RectTransform viewport;
        public GameObject container;
        public GameObject chunksParent;
        public GameObject overlayLayerGameobject;

        Dictionary<Vector2Int, MapChunk> chunks = new Dictionary<Vector2Int, MapChunk>();

        public GameObject mapChunkPrefab;
        private bool chunksInitialized = false;

        public MapChunk GetChunk(int x, int y)
        {
            if (chunks.TryGetValue(new Vector2Int(x, y), out MapChunk value))
            {
                return value;
            }

            return null;
        }

        public void CenterToTile(Vector2 tilePosition)
        {
            if (viewport == null)
                return;

            Vector3 center = viewport.transform.TransformPoint(viewport.rect.center);
            Vector3 delta = center - container.transform.TransformPoint(MapUtils.GetTileToLocalPosition(tilePosition.x, tilePosition.y));

            container.transform.position += delta;
            UpdateCulling();
        }

        [ContextMenu("Force Update Culling")]
        public void UpdateCulling()
        {
            using (var it = chunks.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    it.Current.Value.UpdateCulling();
                }
            }
        }

        private void Start()
        {
            InitializeChunks();
        }

        void InitializeChunks()
        {
            if (chunksInitialized)
                return;

            chunksInitialized = true;
            int tileCoverageX = MapUtils.CHUNK_SIZE.x / MapUtils.PARCEL_SIZE;
            int tileCoverageY = MapUtils.CHUNK_SIZE.y / MapUtils.PARCEL_SIZE;

            int xTile = 0, yTile = 0;

            for (int x = MapUtils.WORLD_PARCELS_OFFSET_MIN.x; x <= MapUtils.WORLD_PARCELS_OFFSET_MAX.x; x += tileCoverageX)
            {
                for (int y = MapUtils.WORLD_PARCELS_OFFSET_MIN.y; y <= MapUtils.WORLD_PARCELS_OFFSET_MAX.y; y += tileCoverageY)
                {
                    var chunk = Object.Instantiate(mapChunkPrefab).GetComponent<MapChunk>();

#if UNITY_EDITOR
                    chunk.gameObject.name = $"Chunk {xTile}, {yTile}";
#endif
                    chunk.transform.SetParent(chunksParent.transform);
                    chunk.transform.localScale = Vector3.one;
                    chunk.transform.localPosition = new Vector3(xTile * MapUtils.CHUNK_SIZE.x, yTile * MapUtils.CHUNK_SIZE.y, 0);

                    //NOTE(Brian): Configure chunk with proper params
                    chunk.center.x = x;
                    chunk.center.y = y;
                    chunk.size.x = MapUtils.CHUNK_SIZE.x;
                    chunk.size.y = MapUtils.CHUNK_SIZE.y;
                    chunk.tileSize = MapUtils.PARCEL_SIZE;
                    (chunk.transform as RectTransform).sizeDelta = new Vector2(MapUtils.CHUNK_SIZE.x, MapUtils.CHUNK_SIZE.y);
                    chunk.owner = this;

                    chunks[new Vector2Int(xTile, yTile)] = chunk;
                    yTile++;
                }

                xTile++;
                yTile = 0;
            }
        }

        public Vector2Int GetTileCoordsFromScreenPosition(float x, float y)
        {
            // x -= WORLD_PARCELS_OFFSET_MIN.x;
            // y -= WORLD_PARCELS_OFFSET_MIN.y;

            // Vector2 result = new Vector2(x / PARCEL_SIZE, y / PARCEL_SIZE) + (Vector2)CENTER_OFFSET;
            // return new Vector2Int((int)result.x, (int)result.y);

            var chunksContainerRect = (chunksParent.transform as RectTransform).rect;
            Vector3 mapMinScreenPosition = chunksContainerRect.min;
            mapMinScreenPosition.z = 0;

            Vector3 mousePosDelta = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 0.1f)) - mapMinScreenPosition;
            mousePosDelta.z = 0;

            return new Vector2Int((int)(mousePosDelta.x / 10), (int)(mousePosDelta.y / 10));
        }
    }
}
