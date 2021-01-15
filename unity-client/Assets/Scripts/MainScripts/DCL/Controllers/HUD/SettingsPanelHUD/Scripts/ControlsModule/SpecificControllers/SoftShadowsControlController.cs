using DCL.SettingsController;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SoftShadows", fileName = "SoftShadowsControlController")]
    public class SoftShadowsControlController : SettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaSoftShadowField = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (lightweightRenderPipelineAsset == null) return;

            lwrpaSoftShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.softShadows;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.softShadows = (bool)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaSoftShadowField?.SetValue(lightweightRenderPipelineAsset, currentQualitySetting.softShadows);

            if (QualitySettingsReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (currentQualitySetting.shadows)
                    shadowType = currentQualitySetting.softShadows ? LightShadows.Soft : LightShadows.Hard;

                QualitySettingsReferences.i.environmentLight.shadows = shadowType;
            }
        }
    }
}