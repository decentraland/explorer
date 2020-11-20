using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD
{
    public class SettingsButtonEntry : MonoBehaviour
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Button button;

        public void Initialize(SettingsButtonData buttonConfig)
        {
            icon.sprite = buttonConfig.icon;
            text.text = buttonConfig.text;
        }

        public void ConfigureAction(UnityAction action)
        {
            button.onClick.AddListener(action);
        }
    }
}