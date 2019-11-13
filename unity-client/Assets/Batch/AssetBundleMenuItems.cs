using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

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

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ORG;
            //AssetBundleBuilder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(10, 10));
            //AssetBundleBuilder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(10, 10));
            //AssetBundleBuilder.DumpArea(new Vector2Int(-90, -110), new Vector2Int(10, 10));
            //AssetBundleBuilder.DumpArea(new Vector2Int(-80, -110), new Vector2Int(10, 10));
            DumpArea(0);
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
    }
}
