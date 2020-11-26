using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlController
    {

    }

    [CreateAssetMenu(menuName = "Settings/Controllers/Control Controller", fileName = "SettingsControlController")]
    public class SettingsControlController : ScriptableObject, ISettingsControlController
    {

    }
}