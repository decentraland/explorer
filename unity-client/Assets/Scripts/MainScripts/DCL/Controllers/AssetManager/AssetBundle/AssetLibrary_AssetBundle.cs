using System.Collections.Generic;

namespace DCL
{
    public class AssetLibrary_AssetBundle : AssetLibrary<Asset_AssetBundle>
    {
        public Dictionary<object, Asset_AssetBundle> masterAssets = new Dictionary<object, Asset_AssetBundle>();

        public override void Add(Asset_AssetBundle asset)
        {
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
            return masterAssets.ContainsKey(asset.id);
        }

        public override Asset_AssetBundle Get(object id)
        {
            if (Contains(id))
            {
                return masterAssets[id];
            }

            return null;
        }

        public override void Release(Asset_AssetBundle asset)
        {

        }
    }
}
