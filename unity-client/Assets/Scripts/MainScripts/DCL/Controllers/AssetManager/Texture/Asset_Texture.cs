using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace DCL
{
    public class Asset_Texture : Asset
    {
        public Texture2D texture;
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

        public void CopyTextureFrom(Texture2D sourceTexture)
        {
            if (this.texture == null)
                Object.Destroy(texture);

            texture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
            Graphics.CopyTexture(sourceTexture, texture);
        }

        public override void Cleanup()
        {
            OnCleanup?.Invoke();
            Object.Destroy(texture);
        }
    }
}