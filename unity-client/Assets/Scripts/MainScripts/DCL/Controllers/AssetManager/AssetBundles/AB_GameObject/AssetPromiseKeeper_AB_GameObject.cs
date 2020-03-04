using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("AssetPromiseKeeper_AssetBundleModelTests")]
namespace DCL
{
    public class AssetPromiseKeeper_AB_GameObject : AssetPromiseKeeper<Asset_AB_GameObject, AssetLibrary_AB_GameObject, AssetPromise_AB_GameObject>
    {
        public AssetPromiseKeeper_AB_GameObject() : base(new AssetLibrary_AB_GameObject())
        {
        }
        protected override void OnSilentForget(AssetPromise_AB_GameObject promise)
        {
            promise.asset.Hide();
        }

        protected override int PromiseSortAlgorithm(AssetPromise_AB_GameObject promiseA, AssetPromise_AB_GameObject promiseB)
        {
            if (promiseA == null || promiseB == null || promiseA.asset == null || promiseB.asset == null)
                return 0;

            float distance1 = Vector3.Distance(promiseA.asset.container.transform.position, CommonScriptableObjects.playerUnityPosition.Get());
            float distance2 = Vector3.Distance(promiseB.asset.container.transform.position, CommonScriptableObjects.playerUnityPosition.Get());

            return (int)distance2 - (int)distance1;
        }

    }

}
