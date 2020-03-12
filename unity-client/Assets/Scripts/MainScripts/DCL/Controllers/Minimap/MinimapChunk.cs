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

        [System.NonSerialized]
        public RectTransform viewport;

        private RectTransform rt;
        private bool isLoadingOrLoaded = false;

        void Start()
        {
            rt = transform as RectTransform;
            targetImage = GetComponent<RawImage>();
        }

        public IEnumerator Load()
        {
            if (isLoadingOrLoaded)
                yield break;

            isLoadingOrLoaded = true;

            string url = $"https://api.decentraland.org/v1/map.png?center={center.x},{center.y}&width={size.x}&height={size.y}&size={tileSize}";

            Texture result = null;

            yield return Utils.FetchTexture(url, (x) => result = x);

            result.filterMode = FilterMode.Trilinear;
            result.wrapMode = TextureWrapMode.Clamp;
            result.anisoLevel = 16;
            ((Texture2D)result).Apply(true, true);

            targetImage.texture = result;
            targetImage.SetNativeSize();
        }

        public void UpdateCulling()
        {
            if (viewport == null)
                return;

            Vector3 myMinCoords = rt.TransformPoint(new Vector3(rt.rect.xMin, rt.rect.yMin));
            Vector3 myMaxCoords = rt.TransformPoint(new Vector3(rt.rect.xMax, rt.rect.yMax));

            Vector3 viewMinCoords = viewport.TransformPoint(new Vector3(viewport.rect.xMin, viewport.rect.yMin));
            Vector3 viewMaxCoords = viewport.TransformPoint(new Vector3(viewport.rect.xMax, viewport.rect.yMax));

            Rect viewportRect = new Rect(viewMinCoords, viewMaxCoords - viewMinCoords);
            viewportRect.min -= Vector2.one * size;
            viewportRect.max += Vector2.one * size;

            Rect myRect = new Rect(myMinCoords, myMaxCoords - myMinCoords);

            bool visible = viewportRect.Overlaps(myRect);

            targetImage.enabled = visible;

            if (!isLoadingOrLoaded && visible)
                CoroutineStarter.Start(Load());
        }
    }
}
