using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[assembly: InternalsVisibleTo("AssetBundleBuilderTests")]
namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [MenuItem("Decentraland/Asset Bundle Builder/Dump Test Scene")]
        public static void DumpMisc()
        {
            AssetBundleBuilder.ExportSceneToAssetBundles_Internal("QmbKgHPENpzGGEfagGP5BbEd7CvqXeXuuXLeMEkuswGvrK");
        }


        [MenuItem("Decentraland/Asset Bundle Builder/Dump Museum District")]
        public static void DumpMuseum()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ORG;
            AssetBundleBuilder.skipAlreadyBuiltBundles = false;
            var zoneArray = AssetBundleBuilderUtils.GetCenteredZoneArray(new Vector2Int(9, 78), new Vector2Int(10, 10));
            AssetBundleBuilder.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/HashTest")]
        public static void GetHash()
        {
            string path2 = AssetDatabase.GUIDToAssetPath("4c57f36c839cb644eb8299a298e7bed9");
            Debug.Log("path2 = " + path2);
            AssetDatabase.DeleteAsset(path2);
            path2 = AssetDatabase.GUIDToAssetPath("4c57f36c839cb644eb8299a298e7bed9");
            Debug.Log("path2 = " + path2);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ORG;
            AssetBundleBuilder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
            //DumpAreaToMax(10, -120);
        }

        static void DumpAreaToMax(int x, int y)
        {
            if (x >= 140 || y >= 140)
                return;

            Debug.Log($"--DumpAreaToMax {x}, {y}");
            int nextX = x + 10;
            int nextY = y;

            if (nextX > 130)
            {
                nextX = -130;
                nextY = y + 10;
            }

            AssetBundleBuilder.DumpArea(new Vector2Int(x, y), new Vector2Int(10, 10), (error) => DumpAreaToMax(nextX, nextY));
        }

        static void DumpArea(int i)
        {
            switch (i)
            {
                case 0:
                    AssetBundleBuilder.DumpArea(new Vector2Int(-110, -100), new Vector2Int(10, 10), (x) => DumpArea(1));
                    break;
                case 1:
                    AssetBundleBuilder.DumpArea(new Vector2Int(-100, -100), new Vector2Int(10, 10), (x) => DumpArea(2));
                    break;
                case 2:
                    AssetBundleBuilder.DumpArea(new Vector2Int(-90, -100), new Vector2Int(10, 10), (x) => DumpArea(3));
                    break;
                case 3:
                    AssetBundleBuilder.DumpArea(new Vector2Int(-80, -100), new Vector2Int(10, 10));
                    break;
            }
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpEverything()
        {
            AssetBundleBuilder.skipAlreadyBuiltBundles = true;
            var zoneArray = AssetBundleBuilderUtils.GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(30, 30));
            AssetBundleBuilder.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            AssetBundleBuilder.finalAssetBundlePath = AssetBundleBuilder.ASSET_BUNDLES_PATH_ROOT;
            BuildPipeline.BuildAssetBundles(AssetBundleBuilder.finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Evaluate Dependency")]
        public static void EvaluateDependency()
        {
            Caching.ClearCache();

            if (Directory.Exists(AssetBundleBuilder.ASSET_BUNDLES_PATH_ROOT))
                Directory.Delete(AssetBundleBuilder.ASSET_BUNDLES_PATH_ROOT, true);

            if (Directory.Exists(AssetBundleBuilder.DOWNLOADED_PATH_ROOT))
                Directory.Delete(AssetBundleBuilder.DOWNLOADED_PATH_ROOT, true);

            AssetDatabase.Refresh();

            AssetBundleBuilder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1), EvaluateDependencyAfterBuild);
        }

        static void EvaluateDependencyAfterBuild(int error)
        {
            if (error != 0)
            {
                Debug.LogError("Error != 0");
                return;
            }

            UnityWebRequest reqDependency = UnityWebRequestAssetBundle.GetAssetBundle(AssetBundleBuilder.ASSET_BUNDLES_PATH_ROOT + "/QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5");

            reqDependency.SendWebRequest();

            while (!reqDependency.isDone) { };

            AssetBundle abDependency = DownloadHandlerAssetBundle.GetContent(reqDependency);

            abDependency.LoadAllAssets();

            UnityWebRequest reqMain = UnityWebRequestAssetBundle.GetAssetBundle(AssetBundleBuilder.ASSET_BUNDLES_PATH_ROOT + "/QmNS4K7GaH63T9rhAfkrra7ADLXSEeco8FTGknkPnAVmKM");

            reqMain.SendWebRequest();

            while (!reqMain.isDone) { };

            AssetBundle abMain = DownloadHandlerAssetBundle.GetContent(reqMain);

            Material[] mats = abMain.LoadAllAssets<Material>();

            bool hasMap = false;

            foreach (var mat in mats)
            {
                if (mat.name.ToLowerInvariant().Contains("mini town"))
                    hasMap = mat.GetTexture("_BaseMap") != null;
            }

            abMain.Unload(true);
            abDependency.Unload(true);

            if (hasMap)
            {
                Debug.Log("Dependency has been generated correctly!");
            }
            else
            {
                Debug.Log("Dependency has NOT been generated correctly!");
            }
        }
    }
}
