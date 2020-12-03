using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling", fileName = "DetailObjectCullingControlController")]
    public class DetailObjectCullingControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentQualitySetting.enableDetailObjectCulling;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.enableDetailObjectCulling = (bool)newValue;
        }
    }
}