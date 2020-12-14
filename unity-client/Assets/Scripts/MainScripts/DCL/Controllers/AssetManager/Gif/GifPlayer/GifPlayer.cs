using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Player for a Gif Asset.
    /// Player will stop if Gif Asset is disposed.
    /// Is not this player responsibility to dispose Gif Asset. Gif Asset should be explicitly disposed.
    /// </summary>
    public class GifPlayer : IDisposable
    {
        public event Action<Texture2D> OnFrameTextureChanged;

        public bool isPlaying { get; private set; } = false;

        private Asset_Gif gifAsset = null;
        private int currentFrameIdx = 0;
        private Coroutine updateRoutine = null;
        private float currentTimeDelay = 0;

        public GifPlayer(Asset_Gif asset)
        {
            SetGif(asset);
        }

        public GifPlayer()
        {
        }

        /// <summary>
        /// Set gif asset for the player
        /// </summary>
        /// <param name="asset">gif asset</param>
        public void SetGif(Asset_Gif asset)
        {
            bool wasPlaying = isPlaying;

            if (gifAsset != null)
            {
                Stop();
                gifAsset.OnCleanup -= OnGifAssetDisposed;
            }

            gifAsset = asset;

            if (asset != null)
            {
                asset.OnCleanup += OnGifAssetDisposed;
                if (wasPlaying) Play();
            }
        }

        public void Play(bool reset = false)
        {
            if (reset)
            {
                currentFrameIdx = 0;
                Stop();
            }

            isPlaying = true;

            if (updateRoutine == null && gifAsset != null)
            {
                updateRoutine = CoroutineStarter.Start(UpdateRoutine());
            }
        }

        public void Stop()
        {
            isPlaying = false;

            if (updateRoutine != null)
            {
                CoroutineStarter.Stop(updateRoutine);
                updateRoutine = null;
            }
        }

        public void Dispose()
        {
            Stop();

            if (gifAsset != null)
            {
                gifAsset.OnCleanup -= OnGifAssetDisposed;
            }
        }

        private void OnGifAssetDisposed()
        {
            Stop();
            gifAsset = null;
        }

        private IEnumerator UpdateRoutine()
        {
            currentTimeDelay = 0;

            while (isPlaying)
            {
                yield return WaitForSecondsCache.Get(currentTimeDelay);

                currentFrameIdx++;

                if (currentFrameIdx >= gifAsset.frames.Length)
                {
                    currentFrameIdx = 0;
                }

                currentTimeDelay = gifAsset.frames[currentFrameIdx].delay;
                OnFrameTextureChanged?.Invoke(gifAsset.frames[currentFrameIdx].texture);
            }
        }
    }
}