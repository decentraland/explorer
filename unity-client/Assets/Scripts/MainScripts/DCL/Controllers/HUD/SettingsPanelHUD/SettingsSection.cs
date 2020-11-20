using UnityEngine;

namespace DCL.SettingsPanelHUD
{
    public class SettingsSection : MonoBehaviour
    {
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}