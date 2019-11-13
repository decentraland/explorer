using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[assembly: InternalsVisibleTo("AssetBundleBuilderTests")]
namespace DCL
{
    public static class AssetBundleBuilderUtils
    {
        [MenuItem("AssetBundleBuilder/Dump Test Scene")]
        public static void DumpMisc()
        {
            AssetBundleBuilder.ExportSceneToAssetBundles_Internal("QmbKgHPENpzGGEfagGP5BbEd7CvqXeXuuXLeMEkuswGvrK");
        }


        [MenuItem("AssetBundleBuilder/Dump Museum District")]
        public static void DumpMuseum()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ORG;
            AssetBundleBuilder.skipUploadedGltfs = false;
            var zoneArray = GetCenteredZoneArray(new Vector2Int(9, 78), new Vector2Int(10, 10));
            AssetBundleBuilder.DumpArea(zoneArray);
        }

        [MenuItem("AssetBundleBuilder/Dump Zone 64,-64")]
        public static void DumpZoneArea()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ZONE;
            AssetBundleBuilder.skipUploadedGltfs = true;
            var zoneArray = GetCenteredZoneArray(new Vector2Int(55, -70), new Vector2Int(15, 15));
            AssetBundleBuilder.DumpArea(new Vector2Int(55, -70), new Vector2Int(15, 15));
        }

        [MenuItem("AssetBundleBuilder/Dump Org 0,0")]
        public static void DumpEverything()
        {
            AssetBundleBuilder.skipUploadedGltfs = true;
            var zoneArray = GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(30, 30));
            AssetBundleBuilder.DumpArea(zoneArray);
        }

        public static List<Vector2Int> GetBottomLeftZoneArray(Vector2Int bottomLeftAnchor, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = bottomLeftAnchor.x; x < bottomLeftAnchor.x + size.x; x++)
            {
                for (int y = bottomLeftAnchor.y; y < bottomLeftAnchor.y + size.y; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            return coords;
        }

        public static List<Vector2Int> GetCenteredZoneArray(Vector2Int center, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = center.x - size.x; x < center.x + size.x; x++)
            {
                for (int y = center.y - size.y; y < center.y + size.y; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            return coords;
        }

        internal static bool ParseOption(string optionName, int argsQty, out string[] foundArgs)
        {
            return ParseOptionExplicit(Environment.GetCommandLineArgs(), optionName, argsQty, out foundArgs);
        }

        internal static bool ParseOptionExplicit(string[] rawArgsList, string optionName, int expectedArgsQty, out string[] foundArgs)
        {
            foundArgs = null;

            if (rawArgsList == null || rawArgsList.Length < expectedArgsQty + 1)
                return false;

            expectedArgsQty = Mathf.Min(expectedArgsQty, 100);

            var foundArgsList = new List<string>();
            int argState = 0;

            for (int i = 0; i < rawArgsList.Length; i++)
            {
                switch (argState)
                {
                    case 0:
                        if (rawArgsList[i] == "-" + optionName)
                        {
                            argState++;
                        }
                        break;
                    default:
                        foundArgsList.Add(rawArgsList[i]);
                        argState++;
                        break;
                }

                if (argState > 0 && foundArgsList.Count == expectedArgsQty)
                    break;
            }

            if (argState == 0 || foundArgsList.Count < expectedArgsQty)
                return false;

            if (expectedArgsQty > 0)
                foundArgs = foundArgsList.ToArray();

            return true;
        }


        [MenuItem("AssetBundleBuilder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            AssetBundleBuilder.finalAssetBundlePath = AssetBundleBuilder.ASSET_BUNDLES_PATH_ROOT;
            BuildPipeline.BuildAssetBundles(AssetBundleBuilder.finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }
    }
}
