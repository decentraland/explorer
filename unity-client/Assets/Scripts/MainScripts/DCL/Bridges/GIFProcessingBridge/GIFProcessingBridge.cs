﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DCL.Helpers;

namespace DCL
{
    /// <summary>
    /// Bridge that handles GIF processing requests for kernel (initiated at Asset_Gif.Load()), and relays the kernel converted textures back to the Asset_Gif
    /// </summary>
    public class GIFProcessingBridge : MonoBehaviour
    {
        [System.Serializable]
        public class UpdateGIFPointersPayload
        {
            public string sceneId;
            public string componentId;
            public int width;
            public int height;
            public int[] pointers;
            public float[] frameDelays;
        }

        class GIFDataContainer
        {
            public UpdateGIFPointersPayload data = null;
        }

        public static GIFProcessingBridge i { get; private set; }

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;
        }

        Dictionary<string, GIFDataContainer> pendingGIFs = new Dictionary<string, GIFDataContainer>();

        /// <summary>
        /// Tells Kernel to start processing a desired GIF, waits for the data to come back from Kernel and passes it to the GIF through the onFinishCallback
        /// </summary>
        /// <param name="onFinishCallback">The callback that will be invoked with the generated textures list</param>
        public IEnumerator RequestGIFProcessor(string url, string sceneId, string componentId, System.Action<List<UniGif.GifTexture>> onFinishCallback)
        {
            var gifDataContainer = new GIFDataContainer();
            string key = sceneId + componentId;

            pendingGIFs.Add(key, gifDataContainer);

            DCL.Interface.WebInterface.RequestGIFProcessor(url, sceneId, componentId, SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2);

            // We use a container class instead of just UpdateGIFPointersPayload to hold its reference and avoid accessing the collection on every yield check
            yield return new WaitUntil(() => gifDataContainer.data != null);

            onFinishCallback?.Invoke(GenerateTexturesList(gifDataContainer.data.width, gifDataContainer.data.height, gifDataContainer.data.pointers, gifDataContainer.data.frameDelays));

            pendingGIFs.Remove(key);
        }

        /// <summary>
        /// Method called by Kernel when a GIF finishes processing on that side. This method populates the pending GIF data that will unlock the waiting RequestGIFProcessor coroutine
        /// </summary>
        public void UpdateGIFPointers(string payload)
        {
            var parsedPayload = Utils.SafeFromJson<UpdateGIFPointersPayload>(payload);

            string key = parsedPayload.sceneId + parsedPayload.componentId;

            if (pendingGIFs.ContainsKey(key))
                pendingGIFs[key].data = parsedPayload;
        }

        public List<UniGif.GifTexture> GenerateTexturesList(int width, int height, int[] pointers, float[] frameDelays)
        {
            if (width == 0 || height == 0)
            {
                Debug.Log("Couldn't create external textures! width or height are 0!");
                return null;
            }

            List<UniGif.GifTexture> gifTexturesList = new List<UniGif.GifTexture>();
            for (int i = 0; i < pointers.Length; i++)
            {
                Texture2D newTex = Texture2D.CreateExternalTexture(width, height, TextureFormat.ARGB32, false, false, (System.IntPtr)pointers[i]);

                if (newTex == null)
                {
                    Debug.Log("Couldn't create external texture!");
                    continue;
                }

                newTex.wrapMode = TextureWrapMode.Clamp;

                gifTexturesList.Add(new UniGif.GifTexture(newTex, frameDelays[i] / 1000));
            }

            return gifTexturesList;
        }
    }
}
