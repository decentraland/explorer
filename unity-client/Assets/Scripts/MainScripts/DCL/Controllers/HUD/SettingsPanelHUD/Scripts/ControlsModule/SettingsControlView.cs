using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SettingsControlView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;

        private ISettingsControlController settingsControlController;

        internal void Initialize(string title, ISettingsControlController settingsControlController)
        {
            this.settingsControlController = settingsControlController;

            this.title.text = title;
        }
    }
}