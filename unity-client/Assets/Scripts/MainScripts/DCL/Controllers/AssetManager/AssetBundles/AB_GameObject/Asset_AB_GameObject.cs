using DCL.Helpers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DCL
{

    public class Asset_AB_GameObject : Asset_WithPoolableContainer
    {
        //NOTE(Brian): Asegurarme de que cuando se cleanupea se propague a los demas siempre usando el AssetPromiseKeeper_AssetBundle
        //             Si tenemos assets trackeados en ambos keepers van a haber problemas.
        AssetPromise_AB ownerPromise;
        public override GameObject container { get; set; }
        public bool alreadyInstantiated;

        public Asset_AB_GameObject()
        {
            alreadyInstantiated = false;
            container = new GameObject("AB Container");
        }

        public override void Cleanup()
        {
            Object.Destroy(container);
        }

        public void Show(System.Action OnFinish)
        {
            if (container == null)
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
            if (alreadyInstantiated)
                yield break;

            alreadyInstantiated = true;
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
