using UnityEngine;

namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        public AssetPromiseKeeper_GLTF() : base(new AssetLibrary_GLTF())
        {
        }

        protected override void OnSilentForget(AssetPromise_GLTF promise)
        {
            promise.asset.Hide();
        }

        protected override int PromiseSortAlgorithm(AssetPromise_GLTF promiseA, AssetPromise_GLTF promiseB)
        {
            float distance1 = Vector3.Distance(promiseA.asset.container.transform.position, CommonScriptableObjects.playerUnityPosition.Get());
            float distance2 = Vector3.Distance(promiseB.asset.container.transform.position, CommonScriptableObjects.playerUnityPosition.Get());

            return (int)distance2 - (int)distance1;
        }

    }
}
