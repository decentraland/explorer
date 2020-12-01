using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlView
    {
        void Initialize(string title, ISettingsControlController settingsControlController);
    }

    public class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;

        protected ISettingsControlController settingsControlController;

        public virtual void Initialize(string title, ISettingsControlController settingsControlController)
        {
            this.settingsControlController = settingsControlController;

            this.title.text = title;
        }
    }
}