using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MapAtlas : MonoBehaviour
    {
        private readonly Vector2Int WORLD_PARCELS_OFFSET_MIN = new Vector2Int(-150, -150);
        private readonly Vector2Int WORLD_PARCELS_OFFSET_MAX = new Vector2Int(175, 175); //NOTE(Brian): We use 175 instead of 150 to make the chunks look even.
        private readonly Vector2Int CHUNK_SIZE = new Vector2Int(1020, 1020);
        private readonly int PARCEL_SIZE = 20;
        private readonly Vector3 CENTER_OFFSET = new Vector3(1, -0.5f);

        public RectTransform viewport;
        public GameObject container;
        Dictionary<Vector2Int, MapChunk> chunks = new Dictionary<Vector2Int, MapChunk>();

        public GameObject mapChunkPrefab;

        public Vector2Int GetTileFromLocalPosition(Vector3 position)
        {
            return new Vector2Int((int)(position.x / PARCEL_SIZE) + WORLD_PARCELS_OFFSET_MIN.x, (int)(position.y / PARCEL_SIZE) + WORLD_PARCELS_OFFSET_MIN.y);
        }

        public Vector3 GetTilePosition(float x, float y)
        {
            x -= WORLD_PARCELS_OFFSET_MIN.x;
            y -= WORLD_PARCELS_OFFSET_MIN.y;

            Vector3 result = new Vector3(x * PARCEL_SIZE, y * PARCEL_SIZE, 0) + CENTER_OFFSET;
            return result;
        }

        [ContextMenu("Center to -10,10")]
        public void CenterTo00()
        {
            CenterToTile(new Vector2Int(-10, 10));
        }

        public void SetViewport(RectTransform reference)
        {
            Vector3 refMin = reference.TransformPoint(new Vector3(reference.rect.xMin, reference.rect.yMin));
            Vector3 refMax = reference.TransformPoint(new Vector3(reference.rect.xMax, reference.rect.yMax));

            Vector3 targetMin = viewport.InverseTransformPoint(refMin);
            Vector3 targetMax = viewport.InverseTransformPoint(refMax);

            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.zero;
            viewport.pivot = Vector2.zero;
            viewport.anchoredPosition = targetMin;
            viewport.sizeDelta = targetMax - targetMin;
        }

        public Vector3 GetViewportCenter()
        {
            Vector3 targetMin = viewport.TransformPoint(new Vector3(viewport.rect.xMin, viewport.rect.yMin));
            Vector3 targetMax = viewport.TransformPoint(new Vector3(viewport.rect.xMin, viewport.rect.yMin));

            return 0.5f * (targetMax + targetMin);
        }

        public void CenterToTile(Vector2 tilePosition)
        {
            Vector3 center = viewport.transform.TransformPoint(viewport.rect.center);
            Vector3 delta = center - container.transform.TransformPoint(GetTilePosition(tilePosition.x, tilePosition.y));

            container.transform.position += delta;
            UpdateCulling();
        }

        private void UpdateCulling()
        {
            using (var it = chunks.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    it.Current.Value.UpdateCulling();
                }
            }
        }

        Vector3 lastPos;
        private void Update()
        {
            if (lastPos != container.transform.position)
            {
                UpdateCulling();
            }

            lastPos = container.transform.position;
        }

        void Start()
        {
            int tileCoverageX = CHUNK_SIZE.x / PARCEL_SIZE;
            int tileCoverageY = CHUNK_SIZE.y / PARCEL_SIZE;

            int xTile = 0, yTile = 0;

            for (int x = WORLD_PARCELS_OFFSET_MIN.x; x <= WORLD_PARCELS_OFFSET_MAX.x; x += tileCoverageX)
            {
                for (int y = WORLD_PARCELS_OFFSET_MIN.y; y <= WORLD_PARCELS_OFFSET_MAX.y; y += tileCoverageY)
                {
                    var chunk = Object.Instantiate(mapChunkPrefab).GetComponent<MapChunk>();

#if UNITY_EDITOR
                    chunk.gameObject.name = $"Chunk {xTile}, {yTile}";
#endif
                    chunk.transform.SetParent(container.transform);
                    chunk.transform.localScale = Vector3.one;
                    chunk.transform.localPosition = new Vector3(xTile * CHUNK_SIZE.x, yTile * CHUNK_SIZE.y, 0);

                    //NOTE(Brian): Configure chunk with proper params
                    chunk.center.x = x;
                    chunk.center.y = y;
                    chunk.size.x = CHUNK_SIZE.x;
                    chunk.size.y = CHUNK_SIZE.y;
                    chunk.tileSize = PARCEL_SIZE;

                    chunk.viewport = viewport;

                    chunks[new Vector2Int(xTile, yTile)] = chunk;
                    yTile++;
                }

                xTile++;
                yTile = 0;
            }
        }
    }
}
