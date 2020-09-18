using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public static class AssetBundleConverter
    {
        public class Settings
        {
            public bool deleteDownloadPathAfterFinished = false;
            public bool skipAlreadyBuiltBundles = false;
            public bool verbose = false;
            public string finalAssetBundlePath;
            public ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG;
        }

        private static Logger log = new Logger(nameof(AssetBundleConverter));

        /// <summary>
        /// Batch-mode entry point
        /// </summary>
        public static void ExportSceneToAssetBundles()
        {
            ExportSceneToAssetBundles(Environment.GetCommandLineArgs());
        }

        public static void ExportSceneToAssetBundles(string[] commandLineArgs)
        {
            Settings settings = new Settings();
            settings.skipAlreadyBuiltBundles = true;
            settings.deleteDownloadPathAfterFinished = true;

            try
            {
                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_SET_CUSTOM_OUTPUT_ROOT_PATH, 1, out string[] outputPath))
                {
                    settings.finalAssetBundlePath = outputPath[0] + "/";
                }

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_SET_CUSTOM_BASE_URL, 1, out string[] customBaseUrl))
                {
                    ContentServerUtils.customBaseUrl = customBaseUrl[0];
                    settings.tld = ContentServerUtils.ApiTLD.NONE;
                }

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_VERBOSE, 0, out _))
                    settings.verbose = true;

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_ALWAYS_BUILD_SYNTAX, 0, out _))
                    settings.skipAlreadyBuiltBundles = false;

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_KEEP_BUNDLES_SYNTAX, 0, out _))
                    settings.deleteDownloadPathAfterFinished = false;

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                {
                    if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                    {
                        throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                    }

                    DumpScene(sceneCid[0], settings);
                    return;
                }

                if (AssetBundleBuilderUtils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_BUILD_PARCELS_RANGE_SYNTAX, 4, out string[] xywh))
                {
                    if (xywh == null)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please use -parcelsXYWH x y w h to establish the desired parcels range to process.");
                    }

                    int x, y, w, h;
                    bool parseSuccess = false;

                    parseSuccess |= int.TryParse(xywh[0], out x);
                    parseSuccess |= int.TryParse(xywh[1], out y);
                    parseSuccess |= int.TryParse(xywh[2], out w);
                    parseSuccess |= int.TryParse(xywh[3], out h);

                    if (!parseSuccess)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please use -parcelsXYWH x y w h to establish the desired parcels range to process.");
                    }

                    if (w > 10 || h > 10 || w < 0 || h < 0)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please don't use negative width/height values, and ensure any given width/height doesn't exceed 10.");
                    }

                    DumpArea(new Vector2Int(x, y), new Vector2Int(w, h), settings);
                    return;
                }

                throw new ArgumentException("Invalid arguments! You must pass -parcelsXYWH or -sceneCid for dump to work!");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
        }

        public static void ConvertScenesToAssetBundles(List<string> sceneCidsList, Settings settings = null)
        {
            if (sceneCidsList == null || sceneCidsList.Count == 0)
            {
                log.Error("Scene list is null or count == 0! Maybe this sector lacks scenes or content requests failed?");
                return;
            }

            log.Info($"Building {sceneCidsList.Count} scenes...");

            List<ContentServerUtils.MappingPair> rawContents = new List<ContentServerUtils.MappingPair>();

            foreach (var sceneCid in sceneCidsList)
            {
                ContentServerUtils.MappingsAPIData parcelInfoApiData = AssetBundleBuilderUtils.GetSceneMappingsData(settings.tld, sceneCid);
                rawContents.AddRange(parcelInfoApiData.data[0].content.contents);
            }

            var core = new AssetBundleConverterCore(EditorEnvironment.CreateWithDefaultImplementations(), settings);
            core.Convert(rawContents.ToArray());
        }

        public static void DumpArea(Vector2Int coords, Vector2Int size, Settings settings)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetSceneCids(settings.tld, coords, size);
            List<string> sceneCidsList = sceneCids.ToList();
            ConvertScenesToAssetBundles(sceneCidsList, settings);
        }

        public static void DumpArea(List<Vector2Int> coords, Settings settings)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetScenesCids(settings.tld, coords);

            List<string> sceneCidsList = sceneCids.ToList();
            ConvertScenesToAssetBundles(sceneCidsList, settings);
        }

        public static void DumpScene(string cid, Settings settings)
        {
            ConvertScenesToAssetBundles(new List<string> {cid}, settings);
        }
    }
}