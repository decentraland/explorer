using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Draw Distance", fileName = "DrawDistanceControlController")]
    public class DrawDistanceControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.cameraDrawDistance;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            currentQualitySetting.cameraDrawDistance = newFloatValue;
            qualitySettingsController.UpdateDrawDistance(newFloatValue);
        }
    }
}