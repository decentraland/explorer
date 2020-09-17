﻿using UnityEditor;

namespace DCL.Wrappers
{
    public interface IAssetDatabase
    {
        void Refresh(ImportAssetOptions options = ImportAssetOptions.Default);
        void SaveAssets();
        void ImportAsset(string path, ImportAssetOptions options = ImportAssetOptions.Default);
        bool DeleteAsset(string path);
        string MoveAsset(string src, string dst);
        void ReleaseCachedFileHandles();
        T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object;
        string GetAssetPath(UnityEngine.Object asset);
        string AssetPathToGUID(string path);
    }
}