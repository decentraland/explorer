using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Distance", fileName = "ShadowDistanceControlController")]
    public class ShadowDistanceControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.shadowDistance;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            currentQualitySetting.shadowDistance = (float)newValue;
            qualitySettingsController.UpdateShadowDistance(newFloatValue);
        }
    }
}