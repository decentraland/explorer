using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DCL.ABConverter;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using UnityGLTF;
using static DCL.ContentServerUtils;
using Utils = DCL.Helpers.Utils;

namespace DCL.ABConverter { }

namespace DCL
{
    public static class AssetBundleMenuItems
    {
        [System.Serializable]
        public class EmptyParcels
        {
            public MappingPair[] EP_01;
            public MappingPair[] EP_02;
            public MappingPair[] EP_03;
            public MappingPair[] EP_04;
            public MappingPair[] EP_05;
            public MappingPair[] EP_06;
            public MappingPair[] EP_07;
            public MappingPair[] EP_08;
            public MappingPair[] EP_09;
            public MappingPair[] EP_10;
            public MappingPair[] EP_11;
            public MappingPair[] EP_12;
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Default Empty Parcels")]
        public static void DumpEmptyParcels_Default() { DumpEmptyParcels(); }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Halloween Empty Parcels")]
        public static void DumpEmptyParcels_Halloween() { DumpEmptyParcels("empty-scenes-halloween"); }

        public static void DumpEmptyParcels(string folderName = "empty-scenes")
        {
            string indexJsonPath = Application.dataPath;

            indexJsonPath += $"/../../kernel/static/loader/{folderName}/index.json";

            if (!File.Exists(indexJsonPath))
            {
                Debug.LogError("Index.json path doesn't exists! Make sure to 'make watch' first so it gets generated.");
                return;
            }

            string emptyScenes = File.ReadAllText(indexJsonPath);
            var es = JsonUtility.FromJson<EmptyParcels>(emptyScenes);

            List<MappingPair> mappings = new List<MappingPair>();

            mappings.AddRange(es.EP_01);
            mappings.AddRange(es.EP_02);
            mappings.AddRange(es.EP_03);
            mappings.AddRange(es.EP_04);
            mappings.AddRange(es.EP_05);
            mappings.AddRange(es.EP_06);
            mappings.AddRange(es.EP_07);
            mappings.AddRange(es.EP_08);
            mappings.AddRange(es.EP_09);
            mappings.AddRange(es.EP_10);
            mappings.AddRange(es.EP_11);
            mappings.AddRange(es.EP_12);

            string emptyScenesResourcesPath = Application.dataPath;
            emptyScenesResourcesPath += $"/../../kernel/static/loader/{folderName}";

            string customBaseUrl = "file://" + emptyScenesResourcesPath;

            var settings = new ABConverter.Client.Settings(ApiTLD.NONE);
            settings.skipAlreadyBuiltBundles = true;
            settings.deleteDownloadPathAfterFinished = false;
            settings.baseUrl = customBaseUrl + "/contents/";

            var core = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            core.Convert(mappings.ToArray());
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Wearables")]
        public static void DumpBaseAvatars()
        {
            var avatarItemList = GetAvatarMappingList("https://wearable-api.decentraland.org/v2/collections");
            var builder = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations());
            
            builder.Convert(ExtractMappingPairs(avatarItemList).ToArray());
        }
        
        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Body-Wearables")]
        public static void DumpAllBodiesWearables()
        {
            List<WearableItem> avatarItemList = GetAvatarMappingList("https://wearable-api.decentraland.org/v2/collections")
                                                .Where(x => x.category == WearableLiterals.Categories.BODY_SHAPE)
                                                .ToList();

            Queue<WearableItem> itemQueue = new Queue<WearableItem>(avatarItemList);
            var settings = new ABConverter.Client.Settings();
            settings.skipAlreadyBuiltBundles = false;
            settings.deleteDownloadPathAfterFinished = false;
            var builder = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            DumpWearableQueue(builder, itemQueue, GLTFImporter_OnBodyWearableLoad);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump All Non-Body-Wearables")]
        public static void DumpAllNonBodiesWearables()
        {
            List<WearableItem> avatarItemList = GetAvatarMappingList("https://wearable-api.decentraland.org/v2/collections")
                                                .Where(x => x.category != WearableLiterals.Categories.BODY_SHAPE)
                                                //.Where(x => x.id == "dcl://dcl_launch/dcl_earrings_earring" || x.id == "dcl://base-avatars/m_feet_soccershoes")
                                                .ToList();
            
            Queue<WearableItem> itemQueue = new Queue<WearableItem>(avatarItemList);
            var settings = new ABConverter.Client.Settings();
            settings.skipAlreadyBuiltBundles = false;
            settings.deleteDownloadPathAfterFinished = false;
            var builder = new ABConverter.Core(ABConverter.Environment.CreateWithDefaultImplementations(), settings);
            DumpWearableQueue(builder, itemQueue, GLTFImporter_OnNonBodyWearableLoad);
        }
        
        private static void DumpWearableQueue(ABConverter.Core builder, Queue<WearableItem> items, System.Action<UnityGLTF.GLTFSceneImporter> OnWearableLoad)
        {
            builder.generateAssetBundles = false;

            if (items.Count == 0)
            {
                AssetBundleManifest manifest;

                if (builder.BuildAssetBundles(out manifest))
                {
                    builder.CleanAssetBundleFolder(manifest.GetAllAssetBundles());
                }

                return;
            }

            Debug.Log("Building wearables... items left... " + items.Count);

            var pairs = ExtractMappingPairs(new List<WearableItem>() { items.Dequeue() });

            UnityGLTF.GLTFImporter.OnGLTFWillLoad += OnWearableLoad;

            builder.Convert(pairs.ToArray(),
                (err) =>
                {
                    UnityGLTF.GLTFImporter.OnGLTFWillLoad -= OnWearableLoad;
                    builder.CleanupWorkingFolders();
                    DumpWearableQueue(builder, items, OnWearableLoad);
                });
        }

        private static void GLTFImporter_OnNonBodyWearableLoad(UnityGLTF.GLTFSceneImporter obj)
        {
            obj.importSkeleton = false;
            obj.maxTextureSize = 512;
        }
        private static void GLTFImporter_OnBodyWearableLoad(UnityGLTF.GLTFSceneImporter obj)
        {
            obj.importSkeleton = true;
            obj.maxTextureSize = 512;
        }

        [MenuItem("Decentraland/Start Visual Tests")]
        public static void StartVisualTests()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(VisualTests.TestConvertedAssets());
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org -110,-110")]
        public static void DumpZoneArea()
        {
            ABConverter.Client.DumpArea(new Vector2Int(-110, -110), new Vector2Int(1, 1));
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Single Asset")]
        public static void DumpSingleAsset()
        {
            // TODO: Make an editor window to setup these values from editor (for other dump-modes as well)
            ABConverter.Client.DumpAsset("QmS9eDwvcEpyYXChz6pFpyWyfyajiXbt6KA4CxQa3JKPGC",
                "models/FloorBaseGrass_01/FloorBaseGrass_01.glb",
                "QmXMzPLZNx5EHiYi3tK9MT5g9HqjAqgyAoZUu2LfAXJcSM");
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org -6,30")]
        public static void DumpOrg()
        {
            ABConverter.Client.DumpArea(new Vector2Int(-6, 30), new Vector2Int(15, 15));
        }
        
        [MenuItem("Decentraland/Asset Bundle Builder/Dump Org 0,0")]
        public static void DumpCenterPlaza()
        {
            var zoneArray = Utils.GetCenteredZoneArray(new Vector2Int(0, 0), new Vector2Int(1, 1));
            ABConverter.Client.DumpArea(zoneArray);
        }

        [MenuItem("Decentraland/Asset Bundle Builder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            BuildPipeline.BuildAssetBundles(ABConverter.Config.ASSET_BUNDLES_PATH_ROOT, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }

        [System.Serializable]
        public class WearableItemArray
        {
            [System.Serializable]
            public class Collection
            {
                public string id;
                public List<WearableItem> wearables;
            }

            public List<Collection> data;
        }
        
        public static List<MappingPair> ExtractMappingPairs(List<WearableItem> wearableItems)
        {
            var result = new List<MappingPair>();

            foreach (var wearable in wearableItems)
            {
                foreach (var representation in wearable.representations)
                {
                    foreach (var datum in representation.contents)
                    {
                        result.Add(datum);
                    }
                }
            }

            return result;
        }

        public static List<WearableItem> GetAvatarMappingList(string url)
        {
            List<WearableItem> result = new List<WearableItem>();

            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                Debug.LogWarning($"Request error! Parcels couldn't be fetched! -- {w.error}");
                return null;
            }

            var avatarApiData = JsonUtility.FromJson<WearableItemArray>("{\"data\":" + w.downloadHandler.text + "}");
            
            foreach (var collection in avatarApiData.data)
            {
                foreach (var wearable in collection.wearables)
                {
                    result.Add(wearable);
                }
            }

            return result;
        }
    }
}