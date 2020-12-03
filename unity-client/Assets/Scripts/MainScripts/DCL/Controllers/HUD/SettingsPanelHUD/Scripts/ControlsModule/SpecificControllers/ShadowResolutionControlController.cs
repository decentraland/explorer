using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Resolution", fileName = "ShadowResolutionControlController")]
    public class ShadowResolutionControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return (int)Mathf.Log((int)currentQualitySetting.shadowResolution, 2) - 8;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.shadowResolution = (UnityEngine.Rendering.Universal.ShadowResolution)(256 << (int)newValue);
            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);
        }
    }
}