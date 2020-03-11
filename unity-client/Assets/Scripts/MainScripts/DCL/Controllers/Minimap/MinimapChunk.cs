using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MinimapChunk : MonoBehaviour
    {
        public Vector2Int center;
        public Vector2Int size;
        public int tileSize;
        RawImage targetImage;
        IEnumerator Start()
        {
            targetImage = GetComponent<RawImage>();

            string url = $"https://api.decentraland.org/v1/map.png?center={center.x},{center.y}&width={size.x}&height={size.y}&size={tileSize}";

            Texture result = null;

            yield return Utils.FetchTexture(url, (x) => result = x);

            result.filterMode = FilterMode.Trilinear;
            result.anisoLevel = 16;
            ((Texture2D)result).Apply(true, true);

            targetImage.texture = result;
            targetImage.SetNativeSize();
        }

    }
}
