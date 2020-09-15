using UnityEngine;
using System;

namespace DCL.SettingsData
{
    [CreateAssetMenu(fileName = "QualitySettings", menuName = "QualitySettings")]
    public class QualitySettingsData : ScriptableObject
    {
        [SerializeField] int defaultPresetIndex = 0;
        [SerializeField] QualitySettings[] settings = null;

        public QualitySettings this[int i]
        {
            get { return settings[i]; }
        }

        public int Length
        {
            get { return settings.Length; }
        }

        public QualitySettings defaultPreset
        {
            get { return settings[defaultPresetIndex]; }
        }

        public int defaultIndex
        {
            get { return defaultPresetIndex; }
        }
    }

    [Serializable]
    public struct QualitySettings
    {
        public enum BaseResolution
        {
            BaseRes_720,
            BaseRes_1080,
            BaseRes_Unlimited
        }

        public string displayName;

        [Tooltip("Base resolution level")] public BaseResolution baseResolution;

        [Tooltip("Controls the global anti aliasing setting")]
        public UnityEngine.Rendering.Universal.MsaaQuality antiAliasing;

        [Tooltip("Scales the camera render target allowing the game to render at a resolution different than native resolution. UI is always rendered at native resolution")] [Range(0.5f, 1)]
        public float renderScale;

        [Tooltip("If enabled the main light can be a shadow casting light")]
        public bool shadows;

        [Tooltip("If enabled pipeline will perform shadow filterin. Otherwise all lights that cast shadows will fallback to perform a single shadow sample")]
        public bool softShadows;

        [Tooltip("Resolution of the main light shadowmap texture")]
        public UnityEngine.Rendering.Universal.ShadowResolution shadowResolution;

        [Tooltip("Camera Far")] [Range(40, 100)]
        public float cameraDrawDistance;

        [Tooltip("Enable bloom post process")] public bool bloom;

        [Tooltip("Enable color grading post process")]
        public bool colorGrading;

        [Tooltip("Shadow Distance")] [Range(30, 100)]
        public float shadowDistance;

        public bool Equals(QualitySettings otherSetting)
        {
            if (baseResolution != otherSetting.baseResolution) return false;
            if (antiAliasing != otherSetting.antiAliasing) return false;
            if (renderScale != otherSetting.renderScale) return false;
            if (shadows != otherSetting.shadows) return false;
            if (softShadows != otherSetting.softShadows) return false;
            if (shadowResolution != otherSetting.shadowResolution) return false;
            if (cameraDrawDistance != otherSetting.cameraDrawDistance) return false;
            if (bloom != otherSetting.bloom) return false;
            if (colorGrading != otherSetting.colorGrading) return false;
            if (shadowDistance != otherSetting.shadowDistance) return false;
            return true;
        }
    }
}