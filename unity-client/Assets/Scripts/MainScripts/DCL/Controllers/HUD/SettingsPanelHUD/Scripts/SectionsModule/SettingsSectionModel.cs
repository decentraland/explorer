using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Section")]
    public class SettingsSectionModel : ScriptableObject
    {
        public Sprite icon;
        public string text;
        public SettingsButtonEntry menuButtonPrefab;
        public SettingsSectionView sectionPrefab;
        public SettingsSectionController sectionController;
        public List<SettingsWidgetModel> widgets;

        public SettingsSectionModel(
            Sprite icon,
            string text,
            SettingsButtonEntry menuButtonPrefab,
            SettingsSectionView sectionPrefab,
            SettingsSectionController sectionController,
            List<SettingsWidgetModel> widgets)
        {
            this.icon = icon;
            this.text = text;
            this.menuButtonPrefab = menuButtonPrefab;
            this.sectionPrefab = sectionPrefab;
            this.sectionController = sectionController;
            this.widgets = widgets;
        }
    }
}