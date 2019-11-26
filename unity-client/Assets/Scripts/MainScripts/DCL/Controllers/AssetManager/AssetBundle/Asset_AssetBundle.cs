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

        public List<T> GetAssetsByExtensions<T>(params string[] extensions)
            where T : Object
        {
            var goList = new List<T>(2);

            for (int i1 = 0; i1 < extensions.Length; i1++)
            {
                string ext = extensions[i1];
                List<Object> assets = null;

                if (assetsByExtension.ContainsKey(ext))
                {
                    assets = assetsByExtension[ext];
                    int glbCount = assets.Count;

                    for (int i = 0; i < glbCount; i++)
                    {
                        Object go = assets[i];

                        if (go is T)
                            goList.Add((T)go);
                    }
                }
            }
            return goList;
        }

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

        public void CancelShow()
        {
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
