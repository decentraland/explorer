using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SettingsWidgetTests_PlayMode : TestsBase
    {
        private const int NUMBER_OF_COLUMNS = 2;
        private const string WIDGET_VIEW_PREFAB_PATH = "Widgets/DefaultSettingsWidgetTemplate";
        private const string CONTROL_VIEW_PREFAB_PATH = "Controls/{controlType}SettingsControlTemplate";

        private SettingsWidgetView widgetView;
        private ISettingsWidgetController widgetController;
        private List<SettingsControlGroup> controlColumnsToCreate = new List<SettingsControlGroup>();

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            for (int i = 0; i < NUMBER_OF_COLUMNS; i++)
            {
                controlColumnsToCreate.Add(new SettingsControlGroup()
                {
                    controls = new SettingsControlList()
                });
            }

            widgetView = Object.Instantiate((GameObject)Resources.Load(WIDGET_VIEW_PREFAB_PATH)).GetComponent<SettingsWidgetView>();
            widgetController = Substitute.For<ISettingsWidgetController>();
        }

        protected override IEnumerator TearDown()
        {
            Object.Destroy(widgetView.gameObject);
            controlColumnsToCreate.Clear();

            yield return base.TearDown();
        }

        [UnityTest]
        [TestCase(0, "Slider", ExpectedResult = null)]
        [TestCase(1, "SpinBox", ExpectedResult = null)]
        [TestCase(0, "Toggle", ExpectedResult = null)]
        public IEnumerator GenerateControlsIntoAWidgetViewCorrectly(int columnIndex, string controlType)
        {
            // Arrange
            SettingsControlView controlViewPrefab = ((GameObject)Resources.Load(CONTROL_VIEW_PREFAB_PATH.Replace("{controlType}", controlType))).GetComponent<SettingsControlView>();

            SettingsControlModel newControlConfig = new SettingsControlModel(
                $"TestControl_Col{columnIndex}",
                controlViewPrefab,
                Substitute.For<SettingsControlController>(),
                new List<BooleanVariable>());


            controlColumnsToCreate[columnIndex].controls.Add(newControlConfig);

            // Act
            widgetView.Initialize("TestWidget", widgetController, controlColumnsToCreate);
            yield return null;

            // Assert
            widgetController.Received(1).AddControl(
                Arg.Any<ISettingsControlView>(),
                Arg.Any<SettingsControlController>(),
                Arg.Any<SettingsControlModel>());
        }
    }
}
