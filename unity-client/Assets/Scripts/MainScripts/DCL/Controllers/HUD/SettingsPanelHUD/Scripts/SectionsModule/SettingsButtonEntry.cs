using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Sections
{
    public class SettingsButtonEntry : MonoBehaviour
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Button button;

        public void Initialize(Sprite icon, string text)
        {
            this.icon.sprite = icon;
            this.text.text = text;
        }

        public void ConfigureAction(UnityAction action)
        {
            button.onClick.AddListener(action);
        }
    }
}