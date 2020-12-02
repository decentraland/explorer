using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [System.Serializable]
    public class SettingsControlGroupList : ReorderableArray<SettingsControlGroup>
    {
    }

    [System.Serializable]
    public class SettingsControlList : ReorderableArray<SettingsControlModel>
    {
    }

    [System.Serializable]
    public class SettingsControlGroup
    {
        [Reorderable]
        public SettingsControlList controls;
    }

    public class SettingsControlModel : ScriptableObject
    {
        public string title;
        public SettingsControlView controlPrefab;
        public SettingsControlController controlController;

        public SettingsControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController)
        {
            this.title = title;
            this.controlPrefab = controlPrefab;
            this.controlController = controlController;
        }
    }
}