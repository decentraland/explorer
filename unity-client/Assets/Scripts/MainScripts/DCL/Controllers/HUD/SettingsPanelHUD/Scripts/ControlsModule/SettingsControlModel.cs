using ReorderableList;
using System.Collections.Generic;
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
        [Header("CONTROL CONFIGURATION")]
        public string title;
        public SettingsControlView controlPrefab;
        public SettingsControlController controlController;
        public List<BooleanVariable> flagsThatDeactivateMe;
        public bool isBeta;
    }
}