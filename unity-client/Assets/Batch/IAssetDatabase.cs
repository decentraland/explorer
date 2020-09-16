using UnityEditor;

namespace DCL
{
    public interface IAssetDatabase
    {
        void Refresh(ImportAssetOptions options = ImportAssetOptions.Default);
        void SaveAssets();
        void ImportAsset(string path, ImportAssetOptions options = ImportAssetOptions.Default);
        void DeleteAsset(string path);
        void MoveAsset(string src, string dst);
        void ReleaseCachedFileHandles();

        T LoadAssetAtPath<T>(string path);
    }
}