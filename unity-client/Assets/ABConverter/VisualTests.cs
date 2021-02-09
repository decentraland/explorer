using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace DCL.ABConverter
{
    public static class VisualTests
    {
        public static void StartVisualTests()
        {
            EditorSceneManager.OpenScene($"Assets/ABConverter/VisualTestScene.unity", OpenSceneMode.Single);


            //var gltfs = LoadAndInstanceAllGltfAssets();
            //var abs = LoadAndInstanceAllAssetBundles();
        }

        public static GameObject[] LoadAndInstanceAllGltfAssets()
        {
            var assets = AssetDatabase.FindAssets("t:GameObject", new[] {"Assets/_Downloaded"});

            List<GameObject> importedGLTFs = new List<GameObject>();

            foreach (var guid in assets)
            {
                GameObject gltf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));

                string hashName = gltf.name;

                var importedGLTF = Object.Instantiate(gltf);
                importedGLTFs.Add(importedGLTF);
            }

            return importedGLTFs.ToArray();
        }

        public static GameObject[] LoadAndInstanceAllAssetBundles()
        {
            Caching.ClearCache();

            string workingFolderName = "_Downloaded";
            string abPath = Application.dataPath + "/../AssetBundles/";

            var pathList = Directory.GetDirectories(Application.dataPath + "/" + workingFolderName);

            List<string> dependencyAbs = new List<string>();
            List<string> mainAbs = new List<string>();

            foreach (var paths in pathList)
            {
                var hash = new DirectoryInfo(paths).Name;
                var path = "Assets/" + workingFolderName + "/" + hash;
                var guids = AssetDatabase.FindAssets("t:GameObject", new[] {path});

                // NOTE(Brian): If no gameObjects are found, we assume they are dependency assets (textures, etc).
                if (guids.Length == 0)
                {
                    dependencyAbs.Add(hash);
                }
                else
                {
                    // Otherwise we assume they are gltfs.
                    mainAbs.Add(hash);
                }
            }

            // NOTE(Brian): We need to store the asset bundles so they can be unloaded later.
            List<AssetBundle> loadedAbs = new List<AssetBundle>();

            foreach (var hash in dependencyAbs)
            {
                string path = abPath + hash;
                var req = UnityWebRequestAssetBundle.GetAssetBundle(path);

                req.SendWebRequest();

                while (!req.isDone)
                {
                }

                var assetBundle = DownloadHandlerAssetBundle.GetContent(req);
                assetBundle.LoadAllAssets();
                loadedAbs.Add(assetBundle);
            }

            List<GameObject> results = new List<GameObject>();

            foreach (var hash in mainAbs)
            {
                string path = abPath + hash;
                var req = UnityWebRequestAssetBundle.GetAssetBundle(path);

                req.SendWebRequest();

                while (!req.isDone)
                {
                }

                var assetBundle = DownloadHandlerAssetBundle.GetContent(req);
                object[] assets = assetBundle.LoadAllAssets();

                foreach (object asset in assets)
                {
                    if (asset is Material material)
                    {
                        material.shader = Shader.Find("DCL/LWRP/Lit");
                    }

                    if (asset is GameObject assetAsGameObject)
                    {
                        results.Add(Object.Instantiate(assetAsGameObject));
                    }
                }

                loadedAbs.Add(assetBundle);
            }

            foreach (var ab in loadedAbs)
            {
                ab.Unload(false);
            }

            return results.ToArray();
        }
    }
}