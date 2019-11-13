using System.Collections.Generic;
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

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone 64,-64")]
        public static void DumpZoneArea()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ORG;
            AssetBundleBuilder.skipUploadedGltfs = true;
            List<System.Action<System.Action<int>>> dumpList = new List<System.Action<System.Action<int>>>();

            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-130, -130), new Vector2Int(10, 10), x));
            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-120, -130), new Vector2Int(10, 10), x));
            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-110, -130), new Vector2Int(10, 10), x));
            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-100, -130), new Vector2Int(10, 10), x));

            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-130, -120), new Vector2Int(10, 10), x));
            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-120, -120), new Vector2Int(10, 10), x));
            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-110, -120), new Vector2Int(10, 10), x));
            dumpList.Add((x) => AssetBundleBuilder.DumpArea(new Vector2Int(-100, -120), new Vector2Int(10, 10), x));

            ChainCall(dumpList, 0);
        }

        static void ChainCall(List<System.Action<System.Action<int>>> list, int index)
        {
            if (index >= list.Count)
                return;

            list[index].Invoke((x) => ChainCall(list, index + 1));
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
