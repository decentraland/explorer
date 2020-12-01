using DCL.SettingsData;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public abstract class SettingsControlController : ScriptableObject
    {
        protected GeneralSettings currentGeneralSettings;

        public virtual void Initialize()
        {
            currentGeneralSettings = Settings.i.generalSettings;
        }

        public abstract object GetStoredValue();

        public abstract void OnControlChanged(object newValue);
    }
}