using System.Collections.Generic;

namespace DCL
{
    public class AssetLibrary_RefCounted<AssetType> : AssetLibrary<AssetType>
        where AssetType : Asset
    {
        public class RefCountedAsset
        {
            public AssetType asset;
            public int referenceCount = 0;
        }

        public Dictionary<object, RefCountedAsset> masterAssets = new Dictionary<object, RefCountedAsset>();
        public Dictionary<AssetType, RefCountedAsset> assetToRefCountedAsset = new Dictionary<AssetType, RefCountedAsset>();

        public override bool Add(AssetType asset)
        {
            if (asset == null || masterAssets.ContainsKey(asset.id))
                return true;

            masterAssets.Add(asset.id, new RefCountedAsset() { asset = asset } );
            assetToRefCountedAsset.Add(asset, masterAssets[asset.id]);
            return true;
        }

        public override void Cleanup()
        {
        }

        public override bool Contains(object id)
        {
            return masterAssets.ContainsKey(id);
        }

        public override bool Contains(AssetType asset)
        {
            if (asset == null)
                return false;

            return masterAssets.ContainsKey(asset.id);
        }

        public override AssetType Get(object id)
        {
            if (!Contains(id))
                return null;

            masterAssets[id].referenceCount++;
            return masterAssets[id].asset;
        }

        public override void Release(AssetType asset)
        {
            if (!Contains(asset))
                return;

            var refCountedAsset = assetToRefCountedAsset[asset]; 
            refCountedAsset.referenceCount--;

            if (refCountedAsset.referenceCount != 0)
                return;

            asset.Cleanup();
            masterAssets.Remove(asset.id);
            assetToRefCountedAsset.Remove(asset);
        }
    }
}
