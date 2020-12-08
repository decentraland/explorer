using DCL.SettingsPanelHUD.Widgets;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    [System.Serializable]
    public class SettingsWidgetList : ReorderableArray<SettingsWidgetModel>
    {
    }

    [CreateAssetMenu(menuName = "Settings/Configuration/Section", fileName = "SectionConfiguration")]
    public class SettingsSectionModel : ScriptableObject
    {
        public Sprite icon;
        public string text;
        public SettingsButtonEntry menuButtonPrefab;
        public SettingsSectionView sectionPrefab;
        public SettingsSectionController sectionController;

        [Reorderable]
        public SettingsWidgetList widgets;
    }
}