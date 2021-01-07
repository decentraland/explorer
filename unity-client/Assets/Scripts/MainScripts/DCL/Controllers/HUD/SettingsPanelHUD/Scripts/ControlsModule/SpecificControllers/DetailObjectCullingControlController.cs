using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling", fileName = "DetailObjectCullingControlController")]
    public class DetailObjectCullingControlController : SettingsControlController
    {
        const string DETAIL_OBJECT_CULLING_SETTINGS_KEY = "Settings.DetailObjectCulling";

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(DETAIL_OBJECT_CULLING_SETTINGS_KEY);
            if (!string.IsNullOrEmpty(storedValue))
                return System.Convert.ToBoolean(storedValue);
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.enableDetailObjectCulling;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;

            Environment.i.platform.cullingController.SetObjectCulling(newBoolValue);
            Environment.i.platform.cullingController.SetShadowCulling(newBoolValue);
            Environment.i.platform.cullingController.MarkDirty();

            CommonSettingsScriptableObjects.detailObjectCullingDisabled.Set(!newBoolValue);
            PlayerPrefs.SetString(DETAIL_OBJECT_CULLING_SETTINGS_KEY, newBoolValue.ToString());
        }
    }
}