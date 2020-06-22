using UnityEngine;

namespace DCL
{
    public class Asset_Texture : Asset
    {
        public Texture2D texture;
        public Asset_Texture dependencyAsset; // to store the default tex asset and release it accordingly

        public override void Cleanup()
        {
            Object.Destroy(texture);
            Debug.Log("destroyed texture in Asset_Texture, reference is now: " + texture);
        }
    }
}
