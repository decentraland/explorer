using System;
using System.Collections;
using DCL.Controllers.Gif;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public enum WrappedTextureMaxSize
    {
        DONT_RESIZE = -1,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048
    }

    public static class WrappedTextureUtils
    {
        public static IEnumerator Fetch(string url, Action<ITexture> OnSuccess,
            WrappedTextureMaxSize maxTextureSize = WrappedTextureMaxSize.DONT_RESIZE)
        {
            string contentType = null;

            var headReq = UnityWebRequest.Head(url);

            yield return headReq.SendWebRequest();

            if (headReq.WebRequestSucceded())
            {
                contentType = headReq.GetResponseHeader("Content-Type");
            }

            yield return Create(contentType, url, maxTextureSize, OnSuccess);
        }

        private static IEnumerator Create(string contentType, string url, WrappedTextureMaxSize maxTextureSize, Action<ITexture> OnSuccess, Action OnFail = null)
        {
            if (contentType == "image/gif")
            {
                byte[] bytes = null;

                yield return Utils.FetchAsset(url, UnityWebRequest.Get(url), (request) => { bytes = request.downloadHandler.data; });

                if (bytes == null)
                {
                    OnFail?.Invoke();
                    yield break;
                }

                var gif = new DCLGif();

                yield return gif.Load(bytes, () =>
                {
                    var wrappedGif = new Asset_Gif(gif);
                    wrappedGif.EnsureTextureMaxSize(maxTextureSize);
                    gif.Play();
                    OnSuccess?.Invoke(wrappedGif);
                });
            }
            else
            {
                AssetPromise_Texture texturePromise = new AssetPromise_Texture(url);
                texturePromise.OnSuccessEvent += texture => { OnSuccess?.Invoke(texture); };

                AssetPromiseKeeper_Texture.i.Keep(texturePromise);
            }
        }
    }

    public class Asset_Gif : ITexture
    {
        DCLGif gif;
        Coroutine updateRoutine = null;

        public Texture2D texture => gif.texture;
        public int width => gif.textureWidth;
        public int height => gif.textureHeight;

        public void Dispose()
        {
            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
            }

            if (gif != null)
            {
                gif.Dispose();
            }
        }

        public void SetUpdateTextureCallback(Action<Texture2D> callback)
        {
            gif.OnFrameTextureChanged += callback;

            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
            }

            updateRoutine = CoroutineStarter.Start(gif.UpdateRoutine());
        }

        public Asset_Gif(DCLGif gif)
        {
            this.gif = gif;
        }

        public void EnsureTextureMaxSize(WrappedTextureMaxSize maxTextureSize)
        {
            if (maxTextureSize != WrappedTextureMaxSize.DONT_RESIZE)
            {
                gif.SetMaxTextureSize((int) maxTextureSize);
            }
        }
    }
}