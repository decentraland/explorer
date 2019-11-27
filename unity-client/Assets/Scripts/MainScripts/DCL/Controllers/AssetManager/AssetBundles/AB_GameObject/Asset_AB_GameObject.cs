using DCL.Helpers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DCL
{

    public class Asset_AB_GameObject : Asset_WithPoolableContainer
    {
        internal AssetPromise_AB ownerPromise;
        public override GameObject container { get; set; }
        public bool isInstantiated;

        public Asset_AB_GameObject()
        {
            isInstantiated = false;
            container = new GameObject("AB Container");
        }

        public override void Cleanup()
        {
            AssetPromiseKeeper_AB.i.Forget(ownerPromise);
            Object.Destroy(container);
        }

        public void Show(System.Action OnFinish)
        {
            if (container == null || ownerPromise == null || ownerPromise.state != AssetPromiseState.FINISHED)
            {
                OnFinish?.Invoke();
                return;
            }

            CoroutineStarter.Start(ShowCoroutine(OnFinish));
        }

        public void Hide()
        {
            container.transform.parent = null;
            container.transform.position = Vector3.one * 5000;
        }

        public IEnumerator ShowCoroutine(System.Action OnFinish)
        {
            yield return InstantiateABGameObjects(ownerPromise.asset.ownerAssetBundle);
            yield return OptimizeMaterials();

            //TODO(Brian): search for GameObject asset and instantiate it.
            OnFinish?.Invoke();
            yield break;
        }

        public IEnumerator OptimizeMaterials()
        {
            MaterialCachingHelper.UseCachedMaterials(container);
            yield break;
        }

        public IEnumerator InstantiateABGameObjects(AssetBundle bundle)
        {
            if (isInstantiated)
                yield break;

            isInstantiated = true;
            var goList = ownerPromise.asset.GetAssetsByExtensions<GameObject>("glb", "ltf");

            for (int i = 0; i < goList.Count; i++)
            {
                GameObject assetBundleModelGO = Object.Instantiate(goList[i]);

                MaterialCachingHelper.UseCachedMaterials(assetBundleModelGO);

                assetBundleModelGO.name = ownerPromise.asset.assetBundleAssetName;
#if UNITY_EDITOR
                assetBundleModelGO.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
                assetBundleModelGO.transform.parent = container.transform;
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
