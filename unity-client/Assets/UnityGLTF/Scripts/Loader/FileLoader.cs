using System;
using System.Collections;
using System.IO;

namespace UnityGLTF.Loader
{
    public class FileLoader : ILoader
    {
        private string _rootDirectoryPath;
        public Stream LoadedStream { get; private set; }

        public bool HasSyncLoadMethod { get; private set; }

        public FileLoader(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
            HasSyncLoadMethod = true;
        }

        public IEnumerator LoadStream(string gltfFilePath)
        {
            if (gltfFilePath == null)
            {
                throw new ArgumentNullException("gltfFilePath");
            }

            yield return LoadFileStream(_rootDirectoryPath, gltfFilePath);
        }

        private IEnumerator LoadFileStream(string rootPath, string fileToLoad)
        {
            string pathToLoad = Path.Combine(rootPath, fileToLoad);
            if (!File.Exists(pathToLoad))
            {
                throw new FileNotFoundException($"Buffer file not found ({pathToLoad})", fileToLoad);
            }

            yield return null;
            LoadedStream = File.OpenRead(pathToLoad);

        }

        public void LoadStreamSync(string gltfFilePath)
        {
            if (gltfFilePath == null)
            {
                throw new ArgumentNullException("gltfFilePath");
            }

            LoadFileStreamSync(_rootDirectoryPath, gltfFilePath);
        }

        private void LoadFileStreamSync(string rootPath, string fileToLoad)
        {
            string pathToLoad = Path.Combine(rootPath, fileToLoad);
            if (!File.Exists(pathToLoad))
            {
                throw new FileNotFoundException("Buffer file not found", fileToLoad);
            }

            LoadedStream = File.OpenRead(pathToLoad);
        }

        public void Dispose()
        {
        }
    }
}
