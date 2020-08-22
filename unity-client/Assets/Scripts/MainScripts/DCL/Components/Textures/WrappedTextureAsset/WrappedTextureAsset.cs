using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers.Gif;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public static class WrappedTextureUtils
    {
        public static IEnumerator Fetch(string url, Action<ITexture, AssetPromise_Texture> OnSuccess, Asset_Gif.MaxSize maxTextureSize = Asset_Gif.MaxSize.DONT_RESIZE,
                                        string sceneId = null, string componentId = null)
        {
            Debug.Log("pravs - WrappedTextureUtils.Fetch()");
            string contentType = null;

            var headReq = UnityWebRequest.Head(url);

            yield return headReq.SendWebRequest();

            if (headReq.WebRequestSucceded())
            {
                contentType = headReq.GetResponseHeader("Content-Type");
            }

            yield return Create(contentType, url, maxTextureSize, sceneId, componentId, OnSuccess);
        }

        private static IEnumerator Create(string contentType, string url, Asset_Gif.MaxSize maxTextureSize, string sceneId, string componentId,
                                        Action<ITexture, AssetPromise_Texture> OnSuccess, Action OnFail = null)
        {
            // Debug.Log("pravs - WrappedTextureUtils.Create() - 1");
            if (contentType != "image/gif")
            {
                // Debug.Log("pravs - WrappedTextureUtils.Create() - 2");
                AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
                texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texture, texturePromise); };
                texturePromise.OnFailEvent += (x) => OnFail?.Invoke();

                // AssetPromiseKeeper_Texture.i.Forget(texturePromise);
                // Debug.Log("pravs - WrappedTextureUtils.Create() - STARTED KEEP: " + url);
                AssetPromiseKeeper_Texture.i.Keep(texturePromise);
                yield return texturePromise;

                yield break;
            }
            // Debug.Log("pravs - WrappedTextureUtils.Create() - 3");

            var gif = new Asset_Gif(url, maxTextureSize, sceneId, componentId, OnSuccess);
            yield return gif.Load();
        }
    }
}