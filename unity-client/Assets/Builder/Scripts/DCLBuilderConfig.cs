using System;
using UnityEngine;

namespace Builder
{
    public static class DCLBuilderConfig
    {
        public static event Action<BuilderConfig> OnConfigChanged;
        public static BuilderConfig config { get; private set; } = BuilderConfig.DefaultBuilderConfig;

        public static void SetConfig(BuilderConfig newBuilderConfig)
        {
            config = newBuilderConfig;
            OnConfigChanged?.Invoke(config);
        }

        public static void SetConfig(string configJson)
        {
            try
            {
                var newConfig = JsonUtility.FromJson<BuilderConfig>(configJson);
                SetConfig(newConfig);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting builder's configuration {e.Message}");
            }
        }
    }

    [Serializable]
    public struct BuilderConfig
    {
        [Serializable]
        public struct Camera
        {
            public float zoomMin;
            public float zoomMax;
        }

        [Serializable]
        public struct Environment
        {
            public bool disableFloor;
        }

        public static BuilderConfig DefaultBuilderConfig
        {
            get
            {
                return new BuilderConfig()
                {
                    camera = new Camera() {zoomMin = 1f, zoomMax = 100f},
                    environment = new Environment() {disableFloor = false}
                };
            }
        }

        public Camera camera;
        public Environment environment;
    }
}