using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCL
{
    public abstract partial class ABConverter
    {
        public static class Client
        {
            public class Settings
            {
                public bool deleteDownloadPathAfterFinished = false;
                public bool skipAlreadyBuiltBundles = false;
                public bool verbose = false;
                public string finalAssetBundlePath = AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT + Path.DirectorySeparatorChar;
                public string baseUrl;

                public Settings Clone()
                {
                    return this.MemberwiseClone() as Settings;
                }
            }

            private static Logger log = new Logger("ABConverter.Client");

            public static EditorEnvironment env;

            public static EditorEnvironment EnsureEnvironment()
            {
                if (env == null)
                    env = EditorEnvironment.CreateWithDefaultImplementations();

                return env;
            }

            /// <summary>
            /// Batch-mode entry point
            /// </summary>
            public static void ExportSceneToAssetBundles()
            {
                EnsureEnvironment();
                ExportSceneToAssetBundles(Environment.GetCommandLineArgs());
            }

            public static void ExportSceneToAssetBundles(string[] commandLineArgs)
            {
                Settings settings = new Settings();
                settings.skipAlreadyBuiltBundles = true;
                settings.deleteDownloadPathAfterFinished = true;
                settings.verbose = true;
                settings.baseUrl = ContentServerUtils.GetBaseUrl(ContentServerUtils.ApiTLD.ORG) + "/contents/";

                try
                {
                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_SET_CUSTOM_OUTPUT_ROOT_PATH, 1, out string[] outputPath))
                    {
                        settings.finalAssetBundlePath = outputPath[0] + "/";
                    }

                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_SET_CUSTOM_BASE_URL, 1, out string[] customBaseUrl))
                        settings.baseUrl = customBaseUrl[0];

                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_VERBOSE, 0, out _))
                        settings.verbose = true;

                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_ALWAYS_BUILD_SYNTAX, 0, out _))
                        settings.skipAlreadyBuiltBundles = false;

                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_KEEP_BUNDLES_SYNTAX, 0, out _))
                        settings.deleteDownloadPathAfterFinished = false;

                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                    {
                        if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                        {
                            throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                        }

                        DumpScene(sceneCid[0], ContentServerUtils.ApiTLD.ORG, settings);
                        return;
                    }

                    if (Utils.ParseOption(commandLineArgs, AssetBundleConverterConfig.CLI_BUILD_PARCELS_RANGE_SYNTAX, 4, out string[] xywh))
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

                        DumpArea(new Vector2Int(x, y), new Vector2Int(w, h), ContentServerUtils.ApiTLD.ORG, settings);
                        return;
                    }

                    throw new ArgumentException("Invalid arguments! You must pass -parcelsXYWH or -sceneCid for dump to work!");
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                }
            }

            public static Core.State ConvertScenesToAssetBundles(List<string> sceneCidsList, ContentServerUtils.ApiTLD tld, Settings settings = null)
            {
                if (sceneCidsList == null || sceneCidsList.Count == 0)
                {
                    log.Error("Scene list is null or count == 0! Maybe this sector lacks scenes or content requests failed?");
                    return new Core.State() {lastErrorCode = Core.ErrorCodes.SCENE_LIST_NULL};
                }

                log.Info($"Building {sceneCidsList.Count} scenes...");

                List<ContentServerUtils.MappingPair> rawContents = new List<ContentServerUtils.MappingPair>();

                foreach (var sceneCid in sceneCidsList)
                {
                    ContentServerUtils.MappingsAPIData parcelInfoApiData = Utils.GetSceneMappingsData(env.webRequest, tld, sceneCid);
                    rawContents.AddRange(parcelInfoApiData.data[0].content.contents);
                }

                var core = new Core(env, settings);
                core.Convert(rawContents.ToArray());
                return core.state;
            }

            public static Core.State DumpArea(Vector2Int coords, Vector2Int size, ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG, Settings settings = null)
            {
                if (settings == null)
                    settings = new Settings();

                HashSet<string> sceneCids = Utils.GetSceneCids(env.webRequest, tld, coords, size);
                List<string> sceneCidsList = sceneCids.ToList();
                return ConvertScenesToAssetBundles(sceneCidsList, tld, settings);
            }

            public static Core.State DumpArea(List<Vector2Int> coords, ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG, Settings settings = null)
            {
                if (settings == null)
                    settings = new Settings();

                HashSet<string> sceneCids = Utils.GetScenesCids(env.webRequest, tld, coords);

                List<string> sceneCidsList = sceneCids.ToList();
                return ConvertScenesToAssetBundles(sceneCidsList, tld, settings);
            }

            public static Core.State DumpScene(string cid, ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG, Settings settings = null)
            {
                if (settings == null)
                    settings = new Settings();

                return ConvertScenesToAssetBundles(new List<string> {cid}, tld, settings);
            }
        }
    }
}