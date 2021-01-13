using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Rendering Scale", fileName = "RenderingScaleControlController")]
    public class RenderingScaleControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.renderScale;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            currentQualitySetting.renderScale = newFloatValue;
            qualitySettingsController.UpdateRenderingScale(newFloatValue);

            ((SliderSettingsControlView)view).OverrideIndicatorLabel(currentQualitySetting.renderScale.ToString("0.0"));
        }
    }
}