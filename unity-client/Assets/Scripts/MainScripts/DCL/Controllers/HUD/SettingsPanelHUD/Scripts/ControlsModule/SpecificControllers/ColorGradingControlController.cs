using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Color Grading", fileName = "ColorGradingControlController")]
    public class ColorGradingControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.colorGrading;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.colorGrading = (bool)newValue;

            Tonemapping toneMapping;
            if (QualitySettingsController.i.postProcessVolume.profile.TryGet<Tonemapping>(out toneMapping))
            {
                toneMapping.active = currentQualitySetting.colorGrading;
            }
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}