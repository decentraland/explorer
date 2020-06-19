using UnityEngine;

namespace DCL
{
    public class Asset_Texture : Asset
    {
        public Texture2D texture;

        public override void Cleanup()
        {
            Object.Destroy(texture);
            Debug.Log("destroyed texture in Asset_Texture, reference is now: " + texture);
        }
    }
}
