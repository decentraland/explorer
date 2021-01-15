using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Distance", fileName = "ShadowDistanceControlController")]
    public class ShadowDistanceControlController : SettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.shadowDistance;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.shadowDistance = (float)newValue;

            if (lightweightRenderPipelineAsset)
                lightweightRenderPipelineAsset.shadowDistance = currentQualitySetting.shadowDistance;
        }
    }
}