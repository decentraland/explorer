using DCL.SettingsPanelHUD.Widgets;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    [System.Serializable]
    public class SettingsWidgetList : ReorderableArray<SettingsWidgetModel>
    {
    }

    [CreateAssetMenu(menuName = "Settings/Configuration/Section")]
    public class SettingsSectionModel : ScriptableObject
    {
        public Sprite icon;
        public string text;
        public SettingsButtonEntry menuButtonPrefab;
        public SettingsSectionView sectionPrefab;
        public SettingsSectionController sectionController;

        [Reorderable]
        public SettingsWidgetList widgets;

        public SettingsSectionModel(
            Sprite icon,
            string text,
            SettingsButtonEntry menuButtonPrefab,
            SettingsSectionView sectionPrefab,
            SettingsSectionController sectionController,
            SettingsWidgetList widgets)
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