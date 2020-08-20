using System;
using System.Collections;
using DCL.Controllers.Gif;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public static class WrappedTextureUtils
    {
        public static IEnumerator Fetch(string url, Action<ITexture> OnSuccess,
            Asset_Gif.MaxSize maxTextureSize = Asset_Gif.MaxSize.DONT_RESIZE, string sceneId = null, string componentId = null)
        {
            string contentType = null;

            var headReq = UnityWebRequest.Head(url);

            yield return headReq.SendWebRequest();

            if (headReq.WebRequestSucceded())
            {
                contentType = headReq.GetResponseHeader("Content-Type");
            }

            yield return Create(contentType, url, maxTextureSize, sceneId, componentId, OnSuccess);
        }

        private static IEnumerator Create(string contentType, string url, Asset_Gif.MaxSize maxTextureSize, string sceneId, string componentId, Action<ITexture> OnSuccess, Action OnFail = null)
        {
            if (contentType != "image/gif")
            {
                AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
                texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texture); };
                texturePromise.OnFailEvent += (x) => OnFail?.Invoke();
                AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                yield break;
            }

            var gif = new Asset_Gif(url, maxTextureSize, sceneId, componentId, null, OnSuccess, null);
            yield return gif.Load();
        }
    }
}