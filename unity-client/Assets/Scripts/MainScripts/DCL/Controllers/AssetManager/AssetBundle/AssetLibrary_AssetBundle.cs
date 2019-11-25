using System.Collections.Generic;

namespace DCL
{
    public class AssetLibrary_AssetBundle : AssetLibrary<Asset_AssetBundle>
    {
        public Dictionary<object, Asset_AssetBundle> masterAssets = new Dictionary<object, Asset_AssetBundle>();

        public override void Add(Asset_AssetBundle asset)
        {
            if (asset == null || masterAssets.ContainsKey(asset.id))
                return;

            masterAssets.Add(asset.id, asset);
        }

        public override void Cleanup()
        {
        }

        public override bool Contains(object id)
        {
            return masterAssets.ContainsKey(id);
        }

        public override bool Contains(Asset_AssetBundle asset)
        {
            if (asset == null)
                return false;

            return masterAssets.ContainsKey(asset.id);
        }

        public override Asset_AssetBundle Get(object id)
        {
            if (!Contains(id))
                return null;

            masterAssets[id].referenceCount++;
            return masterAssets[id];
        }

        public override void Release(Asset_AssetBundle asset)
        {
            asset.referenceCount--;

            if (asset.referenceCount != 0)
                return;

            asset.Cleanup();
            masterAssets.Remove(asset.id);
        }
    }
}
