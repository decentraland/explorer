using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Wrappers;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace DCL
{
    public class EditorEnvironment
    {
        public readonly IDirectory directory;
        public readonly IFile file;
        public readonly IAssetDatabase assetDatabase;
        public readonly IWebRequest webRequest;

        private EditorEnvironment(IDirectory directory, IFile file, IAssetDatabase assetDatabase, IWebRequest webRequest)
        {
            this.directory = directory;
            this.file = file;
            this.assetDatabase = assetDatabase;
            this.webRequest = webRequest;
        }

        public static EditorEnvironment CreateWithDefaultImplementations()
        {
            return new EditorEnvironment
            (
                directory: new Directory(),
                file: new File(),
                assetDatabase: new AssetDatabase(),
                webRequest: new WebRequest_Editor()
            );
        }
    }

    public class WebRequest_Editor : IWebRequest
    {
        private static int ASSET_REQUEST_RETRY_COUNT = 5;

        public void GetAsync(string url, System.Action<byte[]> OnCompleted)
        {
            EditorCoroutineUtility.StartCoroutine(GetAsyncCoroutine(url, OnCompleted), this);
        }

        IEnumerator GetAsyncCoroutine(string url, System.Action<byte[]> OnCompleted)
        {
            UnityWebRequest req;

            int retryCount = ASSET_REQUEST_RETRY_COUNT;

            do
            {
                req = UnityWebRequest.Get(url);
                req.SendWebRequest();

                while (req.isDone == false)
                {
                    yield return null;
                }

                retryCount--;

                if (retryCount == 0)
                {
                    OnCompleted?.Invoke(new byte[0]);
                    yield break;
                }
            } while (!req.WebRequestSucceded());

            OnCompleted?.Invoke(req.downloadHandler.data);
        }

        public byte[] Get(string url)
        {
            UnityWebRequest req;

            int retryCount = ASSET_REQUEST_RETRY_COUNT;

            do
            {
                req = UnityWebRequest.Get(url);
                req.SendWebRequest();
                while (req.isDone == false)
                {
                }

                retryCount--;

                if (retryCount == 0)
                    return null;
            } while (!req.WebRequestSucceded());

            return req.downloadHandler.data;
        }
    }

    public class AssetDatabase : IAssetDatabase
    {
        public void Refresh(ImportAssetOptions options = ImportAssetOptions.Default)
        {
            UnityEditor.AssetDatabase.Refresh(options);
        }

        public void SaveAssets()
        {
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void ImportAsset(string fullPath, ImportAssetOptions options = ImportAssetOptions.Default)
        {
            string assetPath = AssetBundleBuilderUtils.FullPathToAssetPath(fullPath);
            UnityEditor.AssetDatabase.ImportAsset(assetPath, options);
        }

        public bool DeleteAsset(string fullPath)
        {
            string assetPath = AssetBundleBuilderUtils.FullPathToAssetPath(fullPath);
            return UnityEditor.AssetDatabase.DeleteAsset(assetPath);
        }

        public string MoveAsset(string fullPathSrc, string fullPathDst)
        {
            string assetPathSrc = AssetBundleBuilderUtils.FullPathToAssetPath(fullPathSrc);
            string assetPathDst = AssetBundleBuilderUtils.FullPathToAssetPath(fullPathDst);
            return UnityEditor.AssetDatabase.MoveAsset(assetPathSrc, assetPathDst);
        }

        public void ReleaseCachedFileHandles()
        {
            UnityEditor.AssetDatabase.ReleaseCachedFileHandles();
        }

        public T LoadAssetAtPath<T>(string fullPath)
            where T : UnityEngine.Object
        {
            string assetPath = AssetBundleBuilderUtils.FullPathToAssetPath(fullPath);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public string GetAssetPath(Object asset)
        {
            return AssetBundleBuilderUtils.AssetPathToFullPath(UnityEditor.AssetDatabase.GetAssetPath(asset));
        }

        public string AssetPathToGUID(string fullPath)
        {
            string assetPath = AssetBundleBuilderUtils.FullPathToAssetPath(fullPath);
            return UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
        }

        public string GetTextMetaFilePathFromAssetPath(string fullPath)
        {
            string assetPath = AssetBundleBuilderUtils.FullPathToAssetPath(fullPath);
            return UnityEditor.AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);
        }
    }
}