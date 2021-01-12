using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Bloom", fileName = "BloomControlController")]
    public class BloomControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.bloom;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;

            currentQualitySetting.bloom = newBoolValue;
            qualitySettingsController.UpdateBloom(newBoolValue);
        }
    }
}