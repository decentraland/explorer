using DCL.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static DCL.ContentServerUtils;

namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Wearables")]
        public static void DumpBaseAvatars()
        {
            var avatarItemList = GetAvatarMappingList("https://dcl-wearables.now.sh/index.json");
            var builder = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations());
            builder.Convert(avatarItemList);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Zone -110,-110")]
        public static void DumpZoneArea()
        {
            ABConverter.Client.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpCenterPlaza()
        {
            var zoneArray = Utils.GetCenteredZoneArray(new Vector2Int(-119, 135), new Vector2Int(1, 1));
            ABConverter.Client.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Baus Assets")]
        public static void DumpBausAssets()
        {
            var settings = new ABConverter.Client.Settings();
            var env = ABConverter.Environment.CreateWithDefaultImplementations();
            //
            // HashSet<string> sceneCids = ABConverter.Utils.GetScenesCids(env.webRequest, settings.tld, new List<Vector2Int> {new Vector2Int(-119, 135)});
            // var mappingsData = ABConverter.Utils.GetSceneMappingsData(env.webRequest, settings.tld, sceneCids.First());
            //
            // Debug.Log(JsonConvert.SerializeObject(mappingsData));
            //ABConverter.Utils.


            var core = new ABConverter.Core(env, settings);

            var mappingPairs = new List<MappingPair>();
            // mappingPairs.Add(new MappingPair {file = "models/roulette/roulette_table.gltf", hash = "QmfTkJhmoqxPs1xdR3hyPZtGB588kMQZdjobnr11f6H8VG"});
            // mappingPairs.Add(new MappingPair {file = "models/roulette/TominoyaVersion/roulette_table_tominoya_lod.gltf", hash = "QmdpLKUsDaUaFRALCvXLPfvpKwFQPBjZGxVeWv1RSsVev3"});
            // mappingPairs.Add(new MappingPair {file = "models/roulette/chips/CarbonFiber_Specular.png", hash = "QmVycKFL8o42wqsvDdaobiUjJzzuEuXTMsQyDZx9JJp9EB"});
            // mappingPairs.Add(new MappingPair {file = "models/roulette/chips/CarbonFiber_Normal.png", hash = "QmdeaasYE7hVhRGpV6MER1dxDcsomr9itSxKtAyzjD2QH7"});
            // mappingPairs.Add(new MappingPair {file = "models/roulette/roulette_table.bin", hash = "QmWjea2KYpaDSpKP7i6fMrLioizZLcGtBPGWqA7tLunTpo"});
            // mappingPairs.Add(new MappingPair {file = "models/roulette/TominoyaVersion/Roulettemultitex.png", hash = "QmTfNv17regKzrebssqGTXC94T3hdiRg8owV8P9EPu5qEH"});
            // mappingPairs.Add(new MappingPair {file = "models/roulette/TominoyaVersion/roulette_table_tominoya_lod.bin", hash = "QmT7A63xEfbzhF13YZiAKSWiUrFqcqKdZZHSvnA7JyymLA"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/blackjack_table_wood.bin", hash = "QmeKS8eheywSEGUCDi1AAYNEj4dJxqwCEUUsvrGKAz2kgT"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/blackjack_table_wood.gltf", hash = "QmVnc4PZtXMUBWRNoPfyBfsMHBKvFxRtUmCeYDT1iJc9Lb"});
            //mappingPairs.Add(new MappingPair {file = "models/blackjack/blackjack_table_wood_lowPoly.gltf", hash = "QmYYArZwKEcYVZNkv1RKaagVtc8uKwvqfRWyvLJZgo8CLq"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/PokerTableFelt.png", hash = "QmTm31Rmvk9r67mZGnsuwDgvzKp6opLzstDVJPZhhvREmD"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/BlackjackTableTopText.png", hash = "QmbhWV5sMMs3dFE4WouuWVGfpigd2UE6F9nZKyUVrskEuz"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/Wood_TilingSmallDetail_1k_d1.png", hash = "QmSt6pkNUqWRPgwXVRAzYkGCH3QkQUtumRx6Xy8uGiPtNH"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/cards/hearts_low.png", hash = "QmYAHxpZgeAsNTHTsgVzjyXTCV3aFGvhrt6YisDSBqeBSJ"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/cards/hearts_low_png.png", hash = "QmYAHxpZgeAsNTHTsgVzjyXTCV3aFGvhrt6YisDSBqeBSJ"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/blackjacktext.png", hash = "QmaiPWSvrkvz8wHkhR5BP8HNJYxYuLcLYQHzsxMnBwHdR7"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/SpadeArt.png", hash = "Qma6j6xYBjy4cFMSx2TS8s9e1xgBPXCCnC98NN9Dbyu6pf"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/CarbonFiber_Specular.png", hash = "QmVycKFL8o42wqsvDdaobiUjJzzuEuXTMsQyDZx9JJp9EB"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/CarbonFiber_Normal.png", hash = "QmdeaasYE7hVhRGpV6MER1dxDcsomr9itSxKtAyzjD2QH7"});
            mappingPairs.Add(new MappingPair {file = "models/blackjack/SpadeArt.png", hash = "Qma6j6xYBjy4cFMSx2TS8s9e1xgBPXCCnC98NN9Dbyu6pf"});

            core.Convert(mappingPairs.ToArray());
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            BuildPipeline.BuildAssetBundles(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }

        public class WearableItemArray
        {
            public List<WearableItem> data;
        }

        public static MappingPair[] GetAvatarMappingList(string url)
        {
            List<MappingPair> mappingPairs = new List<MappingPair>();

            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone)
            {
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
                        mappingPairs.Add(datum);
                    }
                }
            }

            return mappingPairs.ToArray();
        }
    }
}