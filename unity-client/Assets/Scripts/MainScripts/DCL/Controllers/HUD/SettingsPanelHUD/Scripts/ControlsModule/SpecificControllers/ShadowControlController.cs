using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow", fileName = "ShadowControlController")]
    public class ShadowControlController : ToggleSettingsControlController
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaShadowField = null;

        public override void Initialize(SettingsControlModel controlConfig)
        {
            base.Initialize(controlConfig);

            lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            if (lightweightRenderPipelineAsset == null) return;

            lwrpaShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override object GetStoredValue()
        {
            return currentQualitySetting.shadows;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.shadows = (bool)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowField?.SetValue(lightweightRenderPipelineAsset, currentQualitySetting.shadows);

            if (QualitySettingsReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (currentQualitySetting.shadows)
                    shadowType = currentQualitySetting.shadows ? LightShadows.Soft : LightShadows.Hard;

                QualitySettingsReferences.i.environmentLight.shadows = shadowType;
            }

            CommonSettingsScriptableObjects.shadowsDisabled.Set(!currentQualitySetting.shadows);
        }
    }
}