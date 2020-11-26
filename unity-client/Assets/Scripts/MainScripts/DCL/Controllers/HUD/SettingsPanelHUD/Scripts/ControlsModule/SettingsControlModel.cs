using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [System.Serializable]
    public class SettingsControlList
    {
        public List<SettingsControlModel> controls;
    }

    [System.Serializable]
    public class SettingsControlModel
    {
        [Header("Control configuration")]
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