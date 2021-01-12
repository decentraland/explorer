using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Resolution", fileName = "ShadowResolutionControlController")]
    public class ShadowResolutionControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)Mathf.Log((int)currentQualitySetting.shadowResolution, 2) - 8;
        }

        public override void OnControlChanged(object newValue)
        {
            UnityEngine.Rendering.Universal.ShadowResolution newShadowResValue = (UnityEngine.Rendering.Universal.ShadowResolution)(256 << (int)newValue);

            currentQualitySetting.shadowResolution = newShadowResValue;
            qualitySettingsController.UpdateShadowResolution(newShadowResValue);
        }
    }
}