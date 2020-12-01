using DCL.SettingsPanelHUD.Sections;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD
{
    [System.Serializable]
    public class SettingsSectionList : ReorderableArray<SettingsSectionModel>
    {
    }

    [CreateAssetMenu(menuName = "Settings/Configuration/MainPanel", fileName = "MainPanelConfiguration")]
    public class SettingsPanelModel : ScriptableObject
    {
        [Reorderable]
        public SettingsSectionList sections;
    }
}