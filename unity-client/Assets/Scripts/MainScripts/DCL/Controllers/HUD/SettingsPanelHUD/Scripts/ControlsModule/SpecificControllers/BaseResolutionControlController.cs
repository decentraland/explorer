using DCL.Interface;
using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Base Resolution", fileName = "BaseResolutionControlController")]
    public class BaseResolutionControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)currentQualitySetting.baseResolution;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.baseResolution = (SettingsData.QualitySettings.BaseResolution)newValue;

            switch (currentQualitySetting.baseResolution)
            {
                case SettingsData.QualitySettings.BaseResolution.BaseRes_720:
                    WebInterface.SetBaseResolution(720);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_1080:
                    WebInterface.SetBaseResolution(1080);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_Unlimited:
                    WebInterface.SetBaseResolution(9999);
                    break;
            }
        }
    }
}