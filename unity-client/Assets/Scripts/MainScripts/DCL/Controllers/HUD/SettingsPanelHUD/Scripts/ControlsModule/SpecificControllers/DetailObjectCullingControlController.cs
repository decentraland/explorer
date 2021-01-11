using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling", fileName = "DetailObjectCullingControlController")]
    public class DetailObjectCullingControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.enableDetailObjectCulling;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;
            currentQualitySetting.enableDetailObjectCulling = newBoolValue;

            Environment.i.platform.cullingController.SetObjectCulling(newBoolValue);
            Environment.i.platform.cullingController.SetShadowCulling(newBoolValue);
            Environment.i.platform.cullingController.MarkDirty();

            CommonSettingsScriptableObjects.detailObjectCullingDisabled.Set(!newBoolValue);
        }
    }
}