using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MinimapAtlas : MonoBehaviour
    {
        public RectTransform viewport;
        public GameObject container;
        Dictionary<Vector2Int, MinimapChunk> chunks = new Dictionary<Vector2Int, MinimapChunk>();
        public Vector2Int GetTileFromLocalPosition(Vector3 position)
        {
            return new Vector2Int((int)(position.x / 20) - 150, (int)(position.y / 20) - 150);
        }

        public Vector3 GetTilePosition(float x, float y)
        {
            x += 150;
            y += 150;

            Vector3 offset = new Vector2(1, -0.5f);
            Vector3 result = new Vector3(x * 20, y * 20, 0) + offset;
            result.x /= container.transform.localScale.x;
            result.y /= container.transform.localScale.y;
            return result;
        }

        [ContextMenu("Center to -10,10")]
        public void CenterTo00()
        {
            CenterToTile(new Vector2Int(-10, 10));
        }

        public void CullChunks()
        {

        }

        public void CenterToTile(Vector2Int tilePosition)
        {
            //Vector3 center = viewport.transform.TransformPoint(viewport.rect.center);
            Vector3 center = viewport.transform.position;
            Vector3 delta = center - container.transform.TransformPoint(GetTilePosition(tilePosition.x, tilePosition.y));

            container.transform.position += delta;
        }

        void Start()
        {
            Vector2Int imgSize = new Vector2Int(1020, 1020);
            int tileSize = 20;

            int tileCoverageX = imgSize.x / tileSize;
            int tileCoverageY = imgSize.y / tileSize;

            int xTile = 0, yTile = 0;

            for (int x = -150; x <= 175; x += tileCoverageX)
            {
                for (int y = -150; y <= 175; y += tileCoverageY)
                {
                    var go = new GameObject($"Chunk {xTile}, {yTile}");
                    var chunk = go.AddComponent<MinimapChunk>();
                    go.AddComponent<RawImage>();
                    chunk.transform.parent = container.transform;
                    go.transform.localPosition = new Vector3(xTile * imgSize.x, yTile * imgSize.y, 0) / container.transform.localScale.x;
                    chunk.center.x = x;
                    chunk.center.y = y;
                    chunk.size.x = imgSize.x;
                    chunk.size.y = imgSize.y;
                    chunk.tileSize = tileSize;

                    chunks[new Vector2Int(xTile, yTile)] = chunk;
                    yTile++;
                }
                xTile++;
                yTile = 0;
            }
        }
    }
}
