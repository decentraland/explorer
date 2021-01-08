using DCL.SettingsPanelHUD.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Rendering Scale", fileName = "RenderingScaleControlController")]
    public class RenderingScaleControlController : SettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            if (lightweightRenderPipelineAsset == null)
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.renderScale;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.renderScale = (float)newValue;

            if (lightweightRenderPipelineAsset != null)
            {
                lightweightRenderPipelineAsset.renderScale = currentQualitySetting.renderScale;
            }

            ((SliderSettingsControlView)view).OverrideIndicatorLabel(currentQualitySetting.renderScale.ToString("0.0"));
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}