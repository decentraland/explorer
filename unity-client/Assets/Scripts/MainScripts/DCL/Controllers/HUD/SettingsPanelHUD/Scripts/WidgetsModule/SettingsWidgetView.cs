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
            AdjustWidgetHeight();
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
                    var newWidgetController = Instantiate(controlConfig.controlController);
                    settingsWidgetController.AddControl(newControl, newWidgetController, controlConfig);
                }
            }
        }

        [ContextMenu("AdjustWidgetHeight")]
        public void AdjustWidgetHeight()
        {
            float titleHeight = ((RectTransform)title.transform).sizeDelta.y;

            Transform maxColumn = null;
            float maxColumnHeight = 0f;
            VerticalLayoutGroup columnVerticalLayoutHroup = null;
            foreach (var columnTransform in controlsContainerColumns)
            {
                float columnHeight = 0f;
                for (int i = 0; i < columnTransform.childCount; i++)
                {
                    columnHeight += ((RectTransform) columnTransform.GetChild(i)).sizeDelta.y;
                }

                if (columnHeight > maxColumnHeight)
                {
                    maxColumn = columnTransform;
                    maxColumnHeight = columnHeight;
                    columnVerticalLayoutHroup = columnTransform.GetComponent<VerticalLayoutGroup>();
                }
            }

            ((RectTransform)this.transform).sizeDelta = new Vector2(
                ((RectTransform)this.transform).sizeDelta.x,
                maxColumn != null ?
                    (maxColumnHeight +
                    titleHeight +
                    columnVerticalLayoutHroup.padding.top +
                    columnVerticalLayoutHroup.padding.bottom +
                    (maxColumn.childCount * columnVerticalLayoutHroup.spacing)) :
                    (maxColumnHeight + titleHeight));
        }
    }
}