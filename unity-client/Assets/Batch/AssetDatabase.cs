using UnityEditor;
using UnityEngine;

namespace DCL
{
    public static partial class UnityEditorWrappers
    {
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
                where T : Object
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
                return AssetBundleBuilderUtils.AssetPathToFullPath(UnityEditor.AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath));
            }
        }
    }
}