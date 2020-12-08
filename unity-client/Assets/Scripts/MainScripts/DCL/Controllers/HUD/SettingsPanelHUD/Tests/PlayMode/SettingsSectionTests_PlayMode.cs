using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace SettingsSectionTests
{

    public class SettingsSectionShould_PlayMode : TestsBase
    {
        private const string SECTION_VIEW_PREFAB_PATH = "Sections/DefaultSettingsSectionTemplate";
        private const string WIDGET_VIEW_PREFAB_PATH = "Widgets/DefaultSettingsWidgetTemplate";

        private SettingsSectionView sectionView;
        private ISettingsSectionController sectionController;
        private List<SettingsWidgetModel> widgetsToCreate = new List<SettingsWidgetModel>();

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            sectionView = Object.Instantiate((GameObject)Resources.Load(SECTION_VIEW_PREFAB_PATH)).GetComponent<SettingsSectionView>();
            sectionController = Substitute.For<ISettingsSectionController>();
        }

        protected override IEnumerator TearDown()
        {
            Object.Destroy(sectionView.gameObject);
            widgetsToCreate.Clear();

            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator GenerateWidgetIntoASectionViewCorrectly()
        {
            // Arrange
            SettingsWidgetView widgetViewPrefab = ((GameObject)Resources.Load(WIDGET_VIEW_PREFAB_PATH)).GetComponent<SettingsWidgetView>();

            SettingsWidgetModel newWidgetConfig = new SettingsWidgetModel(
                $"TestWidget",
                widgetViewPrefab,
                new SettingsWidgetController(),
                new SettingsControlGroupList());

            widgetsToCreate.Add(newWidgetConfig);

            // Act
            sectionView.Initialize(sectionController, widgetsToCreate);
            yield return null;

            // Assert
            sectionController.Received(1).AddWidget(
                Arg.Any<ISettingsWidgetView>(),
                Arg.Any<ISettingsWidgetController>(),
                Arg.Any<SettingsWidgetModel>());
        }
    }
}
