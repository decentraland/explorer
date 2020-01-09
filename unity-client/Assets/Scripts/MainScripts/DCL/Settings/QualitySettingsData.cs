﻿using UnityEngine;
using System;

namespace DCL.GameSettings
{
    [CreateAssetMenu(fileName = "QualitySettings", menuName = "QualitySettings")]
    public class QualitySettingsData : ScriptableObject
    {
        [SerializeField] QualitySettings[] settings = null;

        public QualitySettings this[int i]
        {
            get { return settings[i]; }
        }

        public int Length { get { return settings.Length; } }
    }

    [Serializable]
    public struct QualitySettings
    {
        public enum TextureQuality { FullRes = 0, HalfRes, QuarterRes, EighthRes }

        public string displayName;

        [Tooltip("Base texture level")]
        public TextureQuality textureQuality;

        [Tooltip("Controls the global anti aliasing setting")]
        public UnityEngine.Rendering.LWRP.MsaaQuality antiAliasing;

        [Tooltip("Scales the camera render target allowing the game to render at a resolution different than native resolution. UI is always rendered at native resolution")]
        [Range(0.5f, 1)]
        public float renderScale;

        [Tooltip("If enabled the main light can be a shadow casting light")]
        public bool shadows;

        [Tooltip("If enabled pipeline will perform shadow filterin. Otherwise all lights that cast shadows will fallback to perform a single shadow sample")]
        public bool softShadows;

        [Tooltip("Resolution of the main light shadowmap texture")]
        public UnityEngine.Rendering.LWRP.ShadowResolution shadowResolution;

        [Tooltip("Camera Far")]
        [Range(40, 100)]
        public float cameraDrawDistance;

        [Tooltip("Enable bloom post process")]
        public bool bloom;

        [Tooltip("Enable color grading post process")]
        public bool colorGrading;
    }
}