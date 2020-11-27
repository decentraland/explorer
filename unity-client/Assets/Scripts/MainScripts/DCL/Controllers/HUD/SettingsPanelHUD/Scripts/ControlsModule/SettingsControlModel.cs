using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [System.Serializable]
    public class SettingsControlGroup
    {
        public List<SettingsControlModel> controls;
    }

    [CreateAssetMenu(menuName = "Settings/Configuration/Control")]
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