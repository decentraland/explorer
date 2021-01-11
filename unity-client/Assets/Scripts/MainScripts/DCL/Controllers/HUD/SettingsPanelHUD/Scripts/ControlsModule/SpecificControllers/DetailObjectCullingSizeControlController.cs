using DCL.Rendering;
using DCL.SettingsController;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling Size", fileName = "DetailObjectCullingSizeControlController")]
    public class DetailObjectCullingSizeControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.detailObjectCullingThreshold;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.detailObjectCullingThreshold = (float)newValue;

            if (currentQualitySetting.enableDetailObjectCulling)
            {
                var settings = Environment.i.platform.cullingController.GetSettingsCopy();

                settings.rendererProfile = CullingControllerProfile.Lerp(
                    QualitySettingsController.i.cullingControllerSettingsData.rendererProfileMin,
                    QualitySettingsController.i.cullingControllerSettingsData.rendererProfileMax,
                    currentQualitySetting.detailObjectCullingThreshold / 100.0f);

                settings.skinnedRendererProfile = CullingControllerProfile.Lerp(
                    QualitySettingsController.i.cullingControllerSettingsData.skinnedRendererProfileMin,
                    QualitySettingsController.i.cullingControllerSettingsData.skinnedRendererProfileMax,
                    currentQualitySetting.detailObjectCullingThreshold / 100.0f);

                Environment.i.platform.cullingController.SetSettings(settings);
            }
        }
    }
}