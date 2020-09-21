using System.Collections.Generic;
using DCL;
using UnityEditor;
using UnityEngine;

namespace DCL
{
    public sealed partial class MockWrappers
    {
        //TODO(Brian): Use mocking library to replace this mock
        public class AssetDatabase : IAssetDatabase
        {
            public bool refreshed = false;
            public bool saved = false;
            public HashSet<string> importedAssets = new HashSet<string>();
            private Object placeholderObject = new Object();

            public void Refresh(ImportAssetOptions options = ImportAssetOptions.Default)
            {
                refreshed = true;
            }

            public void SaveAssets()
            {
                saved = true;
            }

            public void ImportAsset(string fullPath, ImportAssetOptions options = ImportAssetOptions.Default)
            {
                importedAssets.Add(fullPath);
            }

            public bool DeleteAsset(string path)
            {
                importedAssets.Remove(path);
                return true;
            }

            public string MoveAsset(string src, string dst)
            {
                if (importedAssets.Contains(src))
                {
                    importedAssets.Remove(src);
                    importedAssets.Add(src);
                    return "";
                }

                return "Error";
            }

            public void ReleaseCachedFileHandles()
            {
            }

            public T LoadAssetAtPath<T>(string path) where T : Object
            {
                return placeholderObject as T;
            }

            public string GetAssetPath(Object asset)
            {
                return "";
            }

            public string AssetPathToGUID(string path)
            {
                return "";
            }

            public string GetTextMetaFilePathFromAssetPath(string path)
            {
                return "";
            }
        }
    }
}