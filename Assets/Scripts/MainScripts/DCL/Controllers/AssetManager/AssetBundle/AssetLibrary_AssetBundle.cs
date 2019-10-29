using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AssetLibrary_AssetBundle : AssetLibrary<Asset_AssetBundle>
    {
        public Dictionary<object, Asset_AssetBundle> masterAssets = new Dictionary<object, Asset_AssetBundle>();


        private void OnPoolRemoved(Pool pool)
        {
            pool.OnCleanup -= OnPoolRemoved;
            masterAssets.Remove(pool.id);
        }

        public override void Add(Asset_AssetBundle asset)
        {
            if (asset == null)
            {
                Debug.LogError("asset == null? This shouldn't happen");
                return;
            }

            if (!masterAssets.ContainsKey(asset.id))
                masterAssets.Add(asset.id, asset);

            Pool pool = PoolManager.i.AddPool(asset.id, asset.container);

            pool.OnCleanup -= OnPoolRemoved;
            pool.OnCleanup += OnPoolRemoved;
        }

        public override Asset_AssetBundle Get(object id)
        {
            if (Contains(id))
            {
                Asset_AssetBundle clone = masterAssets[id].Clone() as Asset_AssetBundle;

                if (PoolManager.i.ContainsPool(clone.id))
                {
                    clone.container = PoolManager.i.Get(clone.id).gameObject;
                }
                else
                {
                    Debug.LogError("Pool was removed and AssetLibrary didn't notice?!");
                    return null;
                }

                return clone;
            }

            return null;
        }

        public Asset_AssetBundle GetCopyFromOriginal(object id)
        {
            if (Contains(id))
            {
                Asset_AssetBundle clone = masterAssets[id].Clone() as Asset_AssetBundle;

                if (PoolManager.i.ContainsPool(clone.id))
                {
                    clone.container = PoolManager.i.GetPool(id).Instantiate().gameObject;
                }
                else
                {
                    Debug.LogError("Pool was removed and AssetLibrary didn't notice?!");
                    return null;
                }

                return clone;
            }

            return null;
        }

        public override void Release(Asset_AssetBundle asset)
        {
            if (asset == null)
            {
                Debug.LogError("ERROR: Trying to release null asset");
                return;
            }

            PoolManager.i.Release(asset.container);
        }

        public override bool Contains(object id)
        {
            if (masterAssets == null)
                return false;

            return masterAssets.ContainsKey(id);
        }

        public override bool Contains(Asset_AssetBundle asset)
        {
            if (asset == null)
                return false;

            return Contains(asset.id);
        }

        public override void Cleanup()
        {
            masterAssets.Clear();
        }
    }
}
