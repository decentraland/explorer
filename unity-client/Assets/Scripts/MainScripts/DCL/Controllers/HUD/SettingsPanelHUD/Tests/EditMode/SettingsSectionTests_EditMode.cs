using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace SettingsSectionTests
{
    public class SettingsSectionShould_EditMode
    {
		[Test]
        public void AddWidgetCorrectly()
        {
            // Arrange
            var newWidgetView = Substitute.For<ISettingsWidgetView>();
            var newWidgetController = Substitute.For<ISettingsWidgetController>();
            var newWidgetConfig = new SettingsWidgetModel(
                "TestWidget",
                new SettingsWidgetView(),
                new SettingsWidgetController(),
                new SettingsControlGroupList());

            SettingsSectionController sectionController = new SettingsSectionController();

            // Act
            sectionController.AddWidget(newWidgetView, newWidgetController, newWidgetConfig);

            // Assert
            newWidgetView.Received(1).Initialize(
                newWidgetConfig.title,
                newWidgetController,
                Arg.Any<List<SettingsControlGroup>>());

            Assert.Contains(newWidgetView, sectionController.widgets, "The new widget should be contained in the widget list.");
        }
    }
}
