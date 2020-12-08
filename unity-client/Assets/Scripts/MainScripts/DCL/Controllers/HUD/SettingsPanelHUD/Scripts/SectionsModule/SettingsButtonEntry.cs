using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Sections
{
    public class SettingsButtonEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Color textColorOnSelect;

        [SerializeField]
        private Color backgroundColorOnSelect;

        private Color originalIconColor;
        private Color originalTextColor;
        private Color originalBackgroundColor;
        private bool isSelected;

        public void Initialize(Sprite icon, string text)
        {
            this.icon.sprite = icon;
            this.text.text = text;

            originalIconColor = this.icon.color;
            originalTextColor = this.text.color;
            originalBackgroundColor = backgroundImage.color;
        }

        public void ConfigureAction(UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public void MarkAsSelected(bool isSelected)
        {
            icon.color = isSelected ? textColorOnSelect : originalIconColor;
            text.color = isSelected ? textColorOnSelect : originalTextColor;
            backgroundImage.color = isSelected ? backgroundColorOnSelect : originalBackgroundColor;
            this.isSelected = isSelected;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isSelected)
                return;

            backgroundImage.color = backgroundColorOnSelect;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isSelected)
                return;

            backgroundImage.color = originalBackgroundColor;
        }
    }
}