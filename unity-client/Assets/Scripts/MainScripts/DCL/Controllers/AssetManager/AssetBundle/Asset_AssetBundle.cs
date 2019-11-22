using System.Collections.Generic;
using UnityEngine;

namespace DCL
{

    public class Asset_AssetBundle : Asset
    {
        public int referenceCount = 0;
        public AssetBundle ownerAssetBundle;
        public string assetBundleAssetName;

        public Dictionary<string, Object> assetsByName = new Dictionary<string, Object>();
        public Dictionary<string, List<Object>> assetsByExtension = new Dictionary<string, List<Object>>();

        public Asset_AssetBundle()
        {
        }

        public override object Clone()
        {
            Asset_AssetBundle result = this.MemberwiseClone() as Asset_AssetBundle;
            return result;
        }

        public virtual void Show(bool useMaterialTransition, System.Action OnFinish)
        {
            OnFinish?.Invoke();
        }

        public override void Cleanup()
        {
            if (ownerAssetBundle)
            {
                ownerAssetBundle.Unload(true);
                ownerAssetBundle = null;
            }
        }
    }
}
