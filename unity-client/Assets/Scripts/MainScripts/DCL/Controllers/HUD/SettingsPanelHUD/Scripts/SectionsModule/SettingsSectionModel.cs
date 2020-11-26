using DCL.SettingsPanelHUD.Widgets;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    [System.Serializable]
    public class SettingsSectionModel
    {
        [Header("Menu Button configuration")]
        public Sprite icon;
        public string text;
        public SettingsButtonEntry menuButtonPrefab;

        [Header("Section configuration")]
        public SettingsSectionView sectionPrefab;
        public SettingsSectionController sectionController;

        [Header("Widgets configuration")]
        public SettingsWidgetsConfig widgets;

        public SettingsSectionModel(
            Sprite icon,
            string text,
            SettingsButtonEntry menuButtonPrefab,
            SettingsSectionView sectionPrefab,
            SettingsSectionController sectionController)
        {
            this.icon = icon;
            this.text = text;
            this.menuButtonPrefab = menuButtonPrefab;
            this.sectionPrefab = sectionPrefab;
            this.sectionController = sectionController;
        }
    }
}