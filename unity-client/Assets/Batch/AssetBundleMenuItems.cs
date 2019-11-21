using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("AssetBundleBuilderTests")]
namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [MenuItem("Decentraland/Asset Bundle Builder/Dump Museum District")]
        public static void DumpMuseum()
        {
            var builder = new AssetBundleBuilder();
            builder.environment = ContentServerUtils.ApiEnvironment.ORG;
            builder.skipAlreadyBuiltBundles = false;
            var zoneArray = AssetBundleBuilderUtils.GetCenteredZoneArray(new Vector2Int(9, 78), new Vector2Int(10, 10));
            builder.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump World Borders")]
        public static void DumpBorderArea()
        {
            var builder = new AssetBundleBuilder();
            builder.environment = ContentServerUtils.ApiEnvironment.ORG;
            builder.DumpArea(new Vector2Int(-150, -150), new Vector2Int(10, 5));
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            var builder = new AssetBundleBuilder();
            builder.environment = ContentServerUtils.ApiEnvironment.ORG;
            builder.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
        }

        static void DumpAreaToMax(AssetBundleBuilder builder, int x, int y)
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

            builder.DumpArea(new Vector2Int(x, y), new Vector2Int(10, 10), (error) => DumpAreaToMax(builder, nextX, nextY));
        }


        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpCenterPlaza()
        {
            var builder = new AssetBundleBuilder();
            builder.skipAlreadyBuiltBundles = true;
            var zoneArray = AssetBundleBuilderUtils.GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(30, 30));
            builder.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            BuildPipeline.BuildAssetBundles(AssetBundleBuilderConfig.ASSET_BUNDLES_PATH_ROOT, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }
    }
}
