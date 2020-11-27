using DCL.SettingsPanelHUD.Controls;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Widgets
{
    public interface ISettingsWidgetView
    {
        void Initialize(string title, ISettingsWidgetController settingsWidgetController, SettingsControlsConfig controlsConfig);
    }

    public class SettingsWidgetView : MonoBehaviour, ISettingsWidgetView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private List<Transform> controlsContainerColumns;

        private ISettingsWidgetController settingsWidgetController;
        private SettingsControlsConfig controlsConfig;

        public void Initialize(string title, ISettingsWidgetController settingsWidgetController, SettingsControlsConfig controlsConfig)
        {
            this.settingsWidgetController = settingsWidgetController;
            this.controlsConfig = controlsConfig;

            this.title.text = title;
            CreateControls();
        }

        private void CreateControls()
        {
            Assert.IsTrue(controlsConfig.columns.Count == 0 || controlsConfig.columns.Count == controlsContainerColumns.Count,
                $"[Settings Configuration exception: The number of columns set in the '{controlsConfig.name}' asset does not match with the number of columns set in the '{this.name}' view.");

            for (int columnIndex = 0; columnIndex < controlsConfig.columns.Count; columnIndex++)
            {
                foreach (SettingsControlModel controlConfig in controlsConfig.columns[columnIndex].controls)
                {
                    var newControl = Instantiate(controlConfig.controlPrefab, controlsContainerColumns[columnIndex]);
                    newControl.gameObject.name = $"Control_{controlConfig.title}";
                    var newWidgetController = Instantiate(controlConfig.controlController);
                    settingsWidgetController.AddControl(newControl, newWidgetController, controlConfig);
                }
            }

            AdjustWidgetHeight();
        }

        [ContextMenu("AdjustWidgetHeight")]
        private void AdjustWidgetHeight()
        {
            // Calculate the height of the widget title
            float titleHeight = ((RectTransform)title.transform).sizeDelta.y;

            // Calculate the height of the highest column
            Transform highestColumn = null;
            float highestColumnHeight = 0f;
            foreach (var columnTransform in controlsContainerColumns)
            {
                float columnHeight = 0f;
                for (int controlIndex = 0; controlIndex < columnTransform.childCount; controlIndex++)
                {
                    columnHeight += ((RectTransform)columnTransform.GetChild(controlIndex)).sizeDelta.y;
                }

                if (columnHeight > highestColumnHeight)
                {
                    highestColumn = columnTransform;
                    highestColumnHeight = columnHeight;
                }
            }

            // Calculate the total height of the widget
            float totalHeight = 0f;
            if (highestColumn != null)
            {
                VerticalLayoutGroup columnVerticalLayoutHroup = highestColumn.GetComponent<VerticalLayoutGroup>();

                totalHeight =
                    titleHeight +
                    highestColumnHeight +
                    columnVerticalLayoutHroup.padding.top +
                    columnVerticalLayoutHroup.padding.bottom +
                    (highestColumn.childCount * columnVerticalLayoutHroup.spacing);
            }
            else
            {
                totalHeight = titleHeight + highestColumnHeight;
            }

            // Apply the new widget height
            RectTransform widgetTransform = (RectTransform)this.transform;
            widgetTransform.sizeDelta = new Vector2(widgetTransform.sizeDelta.x, totalHeight);
        }
    }
}