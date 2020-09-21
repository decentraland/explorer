using System;
using System.Collections.Generic;
using System.IO;

namespace DCL
{
    public sealed partial class MockWrappers
    {
        //TODO(Brian): Use mocking library to replace this mock
        public class File : IFile
        {
            public Dictionary<string, string> mockedFiles = new Dictionary<string, string>();

            public void Delete(string path)
            {
                if (Exists(path))
                    mockedFiles.Remove(path);
            }

            public bool Exists(string path)
            {
                return mockedFiles.ContainsKey(path);
            }

            public void Copy(string srcPath, string dstPath)
            {
                if (!Exists(srcPath))
                    throw new FileNotFoundException("Not found!", srcPath);

                mockedFiles.Add(dstPath, mockedFiles[srcPath]);
            }

            public void Move(string srcPath, string dstPath)
            {
                if (!Exists(srcPath))
                    throw new FileNotFoundException("Not found!", srcPath);

                Copy(srcPath, dstPath);
                Delete(srcPath);
            }

            public string ReadAllText(string path)
            {
                if (!Exists(path))
                    throw new FileNotFoundException("Not found!", path);

                return mockedFiles[path];
            }

            public void WriteAllText(string path, string text)
            {
                if (string.IsNullOrEmpty(text))
                    throw new Exception("file contents empty!");

                mockedFiles.Add(path, text);
            }

            public void WriteAllBytes(string path, byte[] bytes)
            {
                if (string.IsNullOrEmpty(path))
                    throw new Exception("path empty!");

                if (bytes == null)
                    throw new Exception("bytes are null!");

                mockedFiles.Add(path, System.Text.Encoding.UTF8.GetString(bytes));
            }

            public Stream OpenRead(string path)
            {
                if (!Exists(path))
                    throw new FileNotFoundException("Not found!", path);

                return GenerateStreamFromString(mockedFiles[path]);
            }

            private static Stream GenerateStreamFromString(string s)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }
    }
}