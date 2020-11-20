using UnityEngine;

namespace DCL.SettingsPanelHUD
{
    [System.Serializable]
    public class SettingsButtonData
    {
        public Sprite icon;
        public string text;
        public SettingsSection sectionToOpen;

        public SettingsButtonData(Sprite icon, string text, SettingsSection sectionToLoad)
        {
            this.icon = icon;
            this.text = text;
            this.sectionToOpen = sectionToLoad;
        }
    }
}