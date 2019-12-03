using DCL.Helpers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static DCL.ContentServerUtils;

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

        [MenuItem("AssetBundleBuilder/Dump All Wearables")]
        public static void DumpBaseAvatars()
        {
            var avatarItemList = GetAvatarMappingList("https://dcl-wearables.now.sh/index.json");
            var builder = new AssetBundleBuilder();
            builder.DownloadAndConvertAssets(avatarItemList);
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

        public class WearableItemArray
        {
            public List<WearableItem> data;
        }

        public static MappingPair[] GetAvatarMappingList(string url)
        {
            List<MappingPair> coords = new List<MappingPair>();

            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone) {
                // TODO: yield this thread?
            }

            if (!w.WebRequestSucceded())
            {
                Debug.LogWarning($"Request error! Parcels couldn't be fetched! -- {w.error}");
                return null;
            }

            var avatarApiData = JsonUtility.FromJson<WearableItemArray>("{\"data\":" + w.downloadHandler.text + "}");

            foreach (var avatar in avatarApiData.data)
            {
                foreach (var representation in avatar.representations)
                {
                    foreach (var datum in representation.contents)
                    {
                        coords.Add(datum);
                    }
                }
            }
            return coords.ToArray();
        }
    }
}
