using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public abstract class SettingsControlController : ScriptableObject
    {
        public abstract void Initialize();

        public abstract object GetStoredValue();

        public abstract void OnControlChanged(object newValue);
    }
}