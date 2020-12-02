using DCL.SettingsData;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Allow Voice Chat", fileName = "AllowVoiceChatControlController")]
    public class AllowVoiceChatControlController : SettingsControlController
    {
        private GeneralSettings currentGeneralSettings;

        public override void Initialize()
        {
            currentGeneralSettings = Settings.i.generalSettings;
        }

        public override object GetStoredValue()
        {
            return (int)currentGeneralSettings.voiceChatAllow;
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.voiceChatAllow = (GeneralSettings.VoiceChatAllow)newValue;
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}