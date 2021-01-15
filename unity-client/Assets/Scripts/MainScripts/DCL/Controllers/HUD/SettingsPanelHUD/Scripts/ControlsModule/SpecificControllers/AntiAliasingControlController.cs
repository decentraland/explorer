using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{

    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/AntiAliasing", fileName = "AntiAliasingControlController")]
    public class AntiAliasingControlController : SliderSettingsControlController
    {
        public const string TEXT_OFF = "OFF";

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize(SettingsControlModel controlConfig)
        {
            base.Initialize(controlConfig);

            if (lightweightRenderPipelineAsset == null)
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            float antiAliasingValue =
                currentQualitySetting.antiAliasing == MsaaQuality.Disabled
                    ? 0
                    : ((int)currentQualitySetting.antiAliasing >> 2) + 1;

            return antiAliasingValue;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            int antiAliasingValue = 1 << (int)newFloatValue;
            currentQualitySetting.antiAliasing = (MsaaQuality)antiAliasingValue;

            if (lightweightRenderPipelineAsset != null)
                lightweightRenderPipelineAsset.msaaSampleCount = antiAliasingValue;

            if (newFloatValue == 0)
                RaiseOnOverrideIndicatorLabel(TEXT_OFF);
            else
                RaiseOnOverrideIndicatorLabel(antiAliasingValue.ToString("0x"));
        }
    }
}