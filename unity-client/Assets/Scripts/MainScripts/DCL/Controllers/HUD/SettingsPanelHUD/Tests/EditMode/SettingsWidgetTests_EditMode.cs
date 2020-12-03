using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{

    public class SettingsWidgetTests_EditMode
    {
		[Test]
        public void AddControlCorrectly()
        {
            // Arrange
            var newControlView = Substitute.For<ISettingsControlView>();
            var newControlController = Substitute.For<SettingsControlController>();
            var newControlConfig = new SettingsControlModel(
                "TestControl",
                Substitute.For<SettingsControlView>(),
                Substitute.For<SettingsControlController>());

            SettingsWidgetController widgetController = new SettingsWidgetController();

            // Act
            widgetController.AddControl(newControlView, newControlController, newControlConfig);

            // Assert
            newControlView.Received(1).Initialize(
                newControlConfig,
                newControlController);

            Assert.Contains(newControlView, widgetController.controls, "The new control should be contained in the control list.");
        }
    }
}
