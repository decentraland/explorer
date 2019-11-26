using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGLTF.Cache;

namespace DCL
{
    public static class MaterialCachingHelper
    {
        public static void UseCachedMaterials(GameObject obj)
        {
            if (obj == null)
                return;

            foreach (var rend in obj.GetComponentsInChildren<Renderer>(true))
            {
                var matList = new List<Material>(1);

                foreach (var mat in rend.sharedMaterials)
                {
                    string crc = mat.ComputeCRC() + mat.name;

                    RefCountedMaterialData refCountedMat;

                    if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                    {
                        mat.enableInstancing = true;
                        PersistentAssetCache.MaterialCacheByCRC.Add(crc, new RefCountedMaterialData(crc, mat));
                    }

                    refCountedMat = PersistentAssetCache.MaterialCacheByCRC[crc];
                    refCountedMat.IncreaseRefCount();

                    matList.Add(refCountedMat.material);
                }

                rend.sharedMaterials = matList.ToArray();
            }
        }
    }

    public class Asset_AssetBundleModel : Asset_AssetBundle
    {
        public GameObject container;
        public bool alreadyInstantiated;

        public Asset_AssetBundleModel()
        {
            alreadyInstantiated = false;
            container = new GameObject("AB Container");
        }

        public override void Cleanup()
        {
            GameObject.Destroy(container);
            base.Cleanup();
        }

        public override void Show(bool useMaterialTransition, System.Action OnFinish)
        {
            assetBundleAssetName = container.name;
            CoroutineStarter.Start(ShowCoroutine(OnFinish));
        }

        public IEnumerator ShowCoroutine(System.Action OnFinish)
        {
            yield return InstantiateABGameObjects(ownerAssetBundle);
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
            var goList = GetAssetsByExtensions<GameObject>("glb", "ltf");

            for (int i = 0; i < goList.Count; i++)
            {
                GameObject assetBundleModelGO = Object.Instantiate(goList[i]);

                MaterialCachingHelper.UseCachedMaterials(assetBundleModelGO);

                assetBundleModelGO.name = assetBundleAssetName;
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
