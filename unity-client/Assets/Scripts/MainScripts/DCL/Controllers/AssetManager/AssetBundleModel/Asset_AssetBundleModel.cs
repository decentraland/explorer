using UnityEngine;

namespace DCL
{
    public class Asset_AssetBundleModel : Asset_AssetBundle
    {
        public GameObject container;

        public override void Show(bool useMaterialTransition, System.Action OnFinish)
        {
            //TODO(Brian): search for GameObject asset and instantiate it.
            OnFinish?.Invoke();
        }

    }
}
