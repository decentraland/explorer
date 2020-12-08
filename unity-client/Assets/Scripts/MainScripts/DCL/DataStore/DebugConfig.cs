using UnityEngine;

namespace DCL
{
    public class DebugConfig
    {
        public bool debugScenes;
        public Vector2Int debugSceneCoords;
        [System.NonSerialized] public bool isDebugMode;
        [System.NonSerialized] public bool isWssDebugMode;
        public bool ignoreGlobalScenes = false;
    }
}