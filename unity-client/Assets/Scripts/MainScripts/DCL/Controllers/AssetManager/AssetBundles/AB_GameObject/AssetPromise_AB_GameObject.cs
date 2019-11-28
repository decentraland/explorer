using System;
using System.Collections;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_AB_GameObject : AssetPromise_WithUrl<Asset_AB_GameObject>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        AssetPromise_AB subPromise;
        Coroutine loadingCoroutine;


        public AssetPromise_AB_GameObject(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        public AssetPromise_AB_GameObject() { }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadingCoroutine = CoroutineStarter.Start(LoadingCoroutine(OnSuccess, OnFail));
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

            //NOTE(Brian): If the asset did load "in world" add it to library and then Get it immediately
            //             So it keeps being there. As master gltfs can't be in the world.
            //
            //             ApplySettings will reposition the newly Get asset to the proper coordinates.
            if (settings.forceNewInstance)
            {
                asset = (library as AssetLibrary_AB_GameObject).GetCopyFromOriginal(asset.id);
            }
            else
            {
                asset = library.Get(asset.id);
            }

            //NOTE(Brian): Call again this method because we are replacing the asset.
            settings.ApplyBeforeLoad(asset.container.transform);

            return true;
        }

        protected override void OnReuse(Action OnSuccess)
        {
            asset.Show(OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
            settings.ApplyAfterLoad(asset.container.transform);
        }

        protected override void OnBeforeLoadOrReuse()
        {
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnCancelLoading()
        {
            CoroutineStarter.Stop(loadingCoroutine);
            AssetPromiseKeeper_AB.i.Forget(subPromise);
        }

        public IEnumerator LoadingCoroutine(Action OnSuccess, Action OnFail)
        {
            subPromise = new AssetPromise_AB(contentUrl, hash);
            bool success = false;
            subPromise.OnSuccessEvent += (x) => success = true;
            asset.ownerPromise = subPromise;
            AssetPromiseKeeper_AB.i.Keep(subPromise);

            yield return subPromise;

            if (success)
            {
                yield return InstantiateABGameObjects(subPromise.asset.ownerAssetBundle);

                if (subPromise.asset == null || subPromise.asset.ownerAssetBundle == null || asset.container == null)
                    success = false;
            }

            if (success)
            {
                OnSuccess?.Invoke();
            }
            else
            {
                OnFail?.Invoke();
            }
        }


        public IEnumerator InstantiateABGameObjects(AssetBundle bundle)
        {
            var goList = subPromise.asset.GetAssetsByExtensions<GameObject>("glb", "ltf");

            for (int i = 0; i < goList.Count; i++)
            {
                if (asset.container == null)
                    break;

                GameObject assetBundleModelGO = UnityEngine.Object.Instantiate(goList[i]);

                MaterialCachingHelper.UseCachedMaterials(assetBundleModelGO);

                assetBundleModelGO.name = subPromise.asset.assetBundleAssetName;
#if UNITY_EDITOR
                assetBundleModelGO.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
                assetBundleModelGO.transform.parent = asset.container.transform;
                assetBundleModelGO.transform.ResetLocalTRS();
                yield return null;
            }

            yield break;
        }

#if UNITY_EDITOR
        private static void ResetShader(Renderer renderer)
        {
            if (renderer.material == null) return;

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials[i].shader = Shader.Find(renderer.materials[i].shader.name);
            }
        }
#endif
    }
}
