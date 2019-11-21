using UnityEngine;

namespace DCL
{
    public class Asset_AssetBundle : Asset
    {
        public GameObject container;
        public AssetBundle ownerAssetBundle;
        public string assetBundleAssetName;

        public Asset_AssetBundle()
        {
            container = new GameObject("AssetBundle");
        }

        public override object Clone()
        {
            Asset_AssetBundle result = this.MemberwiseClone() as Asset_AssetBundle;
            return result;
        }

        public override void Cleanup()
        {
            if (container != null)
                Object.Destroy(container);
        }
    }
}
