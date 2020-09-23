using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                /// <summary>
                /// 
                /// </summary>
                public bool deleteDownloadPathAfterFinished = false;

                /// <summary>
                /// 
                /// </summary>
                public bool skipAlreadyBuiltBundles = false;

                /// <summary>
                /// 
                /// </summary>
                public bool verbose = false;

                /// <summary>
                /// 
                /// </summary>
                public string finalAssetBundlePath = Config.ASSET_BUNDLES_PATH_ROOT + Path.DirectorySeparatorChar;

                /// <summary>
                /// 
                /// </summary>
                public ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG;

                /// <summary>
                /// 
                /// </summary>
                public string baseUrl;

                public Settings Clone()
                {
                    return this.MemberwiseClone() as Settings;
                }

                public Settings(ContentServerUtils.ApiTLD tld = ContentServerUtils.ApiTLD.ORG)
                {
                    this.tld = tld;
                    this.baseUrl = ContentServerUtils.GetContentAPIUrlBase(tld);
                }
            }

            private static Logger log = new Logger("ABConverter.Client");

            public static Environment env;

            public static Environment EnsureEnvironment()
            {
                if (env == null)
                    env = Environment.CreateWithDefaultImplementations();

                return env;
            }

            /// <summary>
            /// Batch-mode entry point
            /// </summary>
            public static void ExportSceneToAssetBundles()
            {
                EnsureEnvironment();
                ExportSceneToAssetBundles(System.Environment.GetCommandLineArgs());
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="commandLineArgs"></param>
            /// <exception cref="ArgumentException"></exception>
            public static void ExportSceneToAssetBundles(string[] commandLineArgs)
            {
                Settings settings = new Settings();

                try
                {
                    if (Utils.ParseOption(commandLineArgs, Config.CLI_SET_CUSTOM_OUTPUT_ROOT_PATH, 1, out string[] outputPath))
                    {
                        settings.finalAssetBundlePath = outputPath[0] + "/";
                    }

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_SET_CUSTOM_BASE_URL, 1, out string[] customBaseUrl))
                        settings.baseUrl = customBaseUrl[0];

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_VERBOSE, 0, out _))
                        settings.verbose = true;

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_ALWAYS_BUILD_SYNTAX, 0, out _))
                        settings.skipAlreadyBuiltBundles = false;

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_KEEP_BUNDLES_SYNTAX, 0, out _))
                        settings.deleteDownloadPathAfterFinished = false;

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                    {
                        if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                        {
                            throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                        }

                        DumpScene(sceneCid[0], settings);
                        return;
                    }

                    if (Utils.ParseOption(commandLineArgs, Config.CLI_BUILD_PARCELS_RANGE_SYNTAX, 4, out string[] xywh))
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

            /// <summary>
            /// This will start the asset bundle conversion for a given scene list, given a scene cids list.
            /// </summary>
            /// <param name="sceneCidsList">The cid list for the scenes to gather from the catalyst's content server</param>
            /// <param name="settings">Any conversion settings object, if its null, a new one will be created</param>
            /// <returns>A state context object useful for tracking the conversion progress</returns>
            public static Core.State ConvertScenesToAssetBundles(List<string> sceneCidsList, Settings settings = null)
            {
                if (sceneCidsList == null || sceneCidsList.Count == 0)
                {
                    log.Error("Scene list is null or count == 0! Maybe this sector lacks scenes or content requests failed?");
                    return new Core.State() {lastErrorCode = Core.ErrorCodes.SCENE_LIST_NULL};
                }

                log.Info($"Building {sceneCidsList.Count} scenes...");

                List<ContentServerUtils.MappingPair> rawContents = new List<ContentServerUtils.MappingPair>();

                EnsureEnvironment();

                if (settings == null)
                    settings = new Settings();

                foreach (var sceneCid in sceneCidsList)
                {
                    ContentServerUtils.MappingsAPIData parcelInfoApiData = ABConverter.Utils.GetSceneMappingsData(env.webRequest, settings.tld, sceneCid);
                    rawContents.AddRange(parcelInfoApiData.data[0].content.contents);
                }

                var core = new ABConverter.Core(env, settings);
                core.Convert(rawContents.ToArray());
                return core.state;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coords"></param>
            /// <param name="size"></param>
            /// <param name="settings"></param>
            /// <returns></returns>
            public static Core.State DumpArea(Vector2Int coords, Vector2Int size, Settings settings = null)
            {
                EnsureEnvironment();

                if (settings == null)
                    settings = new Settings();

                HashSet<string> sceneCids = ABConverter.Utils.GetSceneCids(env.webRequest, settings.tld, coords, size);
                List<string> sceneCidsList = sceneCids.ToList();
                return ConvertScenesToAssetBundles(sceneCidsList, settings);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coords"></param>
            /// <param name="settings"></param>
            /// <returns></returns>
            public static Core.State DumpArea(List<Vector2Int> coords, Settings settings = null)
            {
                EnsureEnvironment();

                if (settings == null)
                    settings = new Settings();

                HashSet<string> sceneCids = Utils.GetScenesCids(env.webRequest, settings.tld, coords);

                List<string> sceneCidsList = sceneCids.ToList();
                return ConvertScenesToAssetBundles(sceneCidsList, settings);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="cid"></param>
            /// <param name="settings"></param>
            /// <returns></returns>
            public static Core.State DumpScene(string cid, Settings settings = null)
            {
                EnsureEnvironment();

                if (settings == null)
                    settings = new Settings();

                return ConvertScenesToAssetBundles(new List<string> {cid}, settings);
            }
        }
    }
}