using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
            currentQualitySetting.bloom = (bool)newValue;

            if (QualitySettingsController.i.postProcessVolume)
            {
                if (QualitySettingsController.i.postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
                {
                    bloom.active = currentQualitySetting.bloom;
                }
            }
        }
    }
}