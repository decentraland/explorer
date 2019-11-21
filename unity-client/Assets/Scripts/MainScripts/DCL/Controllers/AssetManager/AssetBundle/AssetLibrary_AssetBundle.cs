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
            throw new System.NotImplementedException();
        }

        public override bool Contains(Asset_AssetBundle asset)
        {
            throw new System.NotImplementedException();
        }

        public override Asset_AssetBundle Get(object id)
        {
            throw new System.NotImplementedException();
        }

        public override void Release(Asset_AssetBundle asset)
        {
            throw new System.NotImplementedException();
        }
    }
}
