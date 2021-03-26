using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SSAO Controller", fileName = "SSAOControlController")]
    public class SSAOControlController : SpinBoxSettingsControlController
    {
        private UniversalRenderPipelineAsset urpAsset = null;
        private ScriptableRendererFeature ssaoFeature;

        private object settings;
        private FieldInfo sourceField;
        private FieldInfo downsampleField;

        enum QualityLevel
        {
            OFF,
            LOW,
            MID,
            HIGH
        }

        public override void Initialize()
        {
            base.Initialize();

            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            ScriptableRenderer forwardRenderer = urpAsset.GetRenderer(0) as ScriptableRenderer;
            var featuresField = typeof(ScriptableRenderer).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            IList features = featuresField.GetValue(forwardRenderer) as IList;
            ssaoFeature = features[0] as ScriptableRendererFeature;

            FieldInfo settingsField = ssaoFeature.GetType().GetField("m_Settings", BindingFlags.NonPublic | BindingFlags.Instance);
            settings = settingsField.GetValue(ssaoFeature);

            sourceField = settings.GetType().GetField("Source", BindingFlags.NonPublic | BindingFlags.Instance);
            downsampleField = settings.GetType().GetField("Downsample", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private int currentQualityLevel = 0;

        public override object GetStoredValue()
        {
            return currentQualityLevel;
        }

        public override void UpdateSetting(object newValue)
        {
            int value = (int)newValue;
            switch ( value )
            {
                case (int)QualityLevel.OFF:
                    ssaoFeature.SetActive(false);
                    break;
                case (int)QualityLevel.LOW:
                    ssaoFeature.SetActive(true);
                    sourceField.SetValue(settings, 0);
                    downsampleField.SetValue(settings, true);
                    break;
                case (int)QualityLevel.MID:
                    ssaoFeature.SetActive(true);
                    sourceField.SetValue(settings, 1);
                    downsampleField.SetValue(settings, true);
                    break;
                case (int)QualityLevel.HIGH:
                    ssaoFeature.SetActive(true);
                    sourceField.SetValue(settings, 1);
                    downsampleField.SetValue(settings, false);
                    break;
            }

            currentQualityLevel = value;
        }
    }
}