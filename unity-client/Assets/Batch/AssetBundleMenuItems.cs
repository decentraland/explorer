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
            AssetBundleBuilder.skipUploadedGltfs = false;
            var zoneArray = AssetBundleBuilderUtils.GetCenteredZoneArray(new Vector2Int(9, 78), new Vector2Int(10, 10));
            AssetBundleBuilder.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ORG;
            AssetBundleBuilder.skipUploadedGltfs = false;
            AssetBundleBuilder.DumpArea(new Vector2Int(-110, -100), new Vector2Int(10, 10));
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpEverything()
        {
            AssetBundleBuilder.skipUploadedGltfs = true;
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
