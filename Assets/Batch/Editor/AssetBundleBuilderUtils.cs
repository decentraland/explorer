using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
            AssetBundleBuilder.DumpArea(new List<Vector2Int>() { new Vector2Int(13, 75) });
        }

        [MenuItem("AssetBundleBuilder/Dump Zone 64,-64")]
        public static void DumpZoneArea()
        {
            AssetBundleBuilder.environment = ContentServerUtils.ApiEnvironment.ZONE;
            var zoneArray = GetCenteredZoneArray(new Vector2Int(64, -64), new Vector2Int(7, 7));
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

            if (rawArgsList.Length > expectedArgsQty + 1)
            {
                return false;
            }

            var foundArgsList = new List<string>();
            int argState = 0;

            for (int i = 0; i < rawArgsList.Length - expectedArgsQty; i++)
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
            }

            if (argState == 0)
                return false;

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
