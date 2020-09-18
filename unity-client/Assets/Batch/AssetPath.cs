using DCL.Helpers;
using System.Collections;
using System.IO;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine.Networking;
using MappingPair = DCL.ContentServerUtils.MappingPair;


namespace DCL
{
    public class AssetPath
    {
        public readonly string basePath;
        public readonly MappingPair pair;
        public string hash => pair.hash;
        public string file => pair.file;

        public AssetPath(string basePath, MappingPair pair)
        {
            this.basePath = basePath;
            this.pair = pair;
        }

        public string finalPath
        {
            get
            {
                char dash = Path.DirectorySeparatorChar;
                string fileExt = Path.GetExtension(pair.file);
                return basePath + pair.hash + dash + pair.hash + fileExt;
            }
        }
    }
}