using DCL.SettingsData;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlController
    {
        object GetStoredValue();
        void OnControlChanged(object newValue);
    }

    public class SettingsControlController : ScriptableObject, ISettingsControlController
    {
        protected GeneralSettings currentGeneralSettings;

        private void Awake()
        {
            currentGeneralSettings = Settings.i.generalSettings;
        }

        public virtual object GetStoredValue()
        {
            return null;
        }

        public virtual void OnControlChanged(object newValue)
        {
        }
    }
}