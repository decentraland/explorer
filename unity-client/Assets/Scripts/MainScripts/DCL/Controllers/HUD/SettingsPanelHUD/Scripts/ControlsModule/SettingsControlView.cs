using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlView
    {
        void Initialize(string title, SettingsControlController settingsControlController);
    }

    public class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;

        protected SettingsControlController settingsControlController;

        public virtual void Initialize(string title, SettingsControlController settingsControlController)
        {
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize();

            this.title.text = title;
        }
    }
}