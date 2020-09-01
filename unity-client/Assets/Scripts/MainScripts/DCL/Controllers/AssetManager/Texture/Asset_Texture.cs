using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace DCL
{
    public interface ITexture : IDisposable
    {
        Texture2D texture { get; }
        int width { get; }
        int height { get; }
    }

    public class Asset_Texture : Asset, ITexture
    {
        public Texture2D texture { get; set; }
        public Asset_Texture dependencyAsset; // to store the default tex asset and release it accordingly

        public event System.Action OnCleanup;

        public void ConfigureTexture(TextureWrapMode textureWrapMode, FilterMode textureFilterMode, bool makeNoLongerReadable = true)
        {
            if (texture == null) return;

            texture.wrapMode = textureWrapMode;
            texture.filterMode = textureFilterMode;
            texture.Compress(false);
            texture.Apply(textureFilterMode != FilterMode.Point, makeNoLongerReadable);
        }

        public override void Cleanup()
        {
            OnCleanup?.Invoke();
            Object.Destroy(texture);
        }

        public void Dispose()
        {
            Cleanup();
        }

        public int width => texture.width;
        public int height => texture.height;
    }
}