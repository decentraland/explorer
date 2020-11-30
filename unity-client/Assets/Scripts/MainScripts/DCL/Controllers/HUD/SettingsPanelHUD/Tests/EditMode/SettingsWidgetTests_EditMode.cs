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
            var newControlController = Substitute.For<ISettingsControlController>();
            var newControlConfig = new SettingsControlModel(
                "TestControl",
                new SettingsControlView(),
                new SettingsControlController());

            SettingsWidgetController widgetController = new SettingsWidgetController();

            // Act
            widgetController.AddControl(newControlView, newControlController, newControlConfig);

            // Assert
            newControlView.Received(1).Initialize(
                newControlConfig.title,
                newControlController);

            Assert.Contains(newControlView, widgetController.controls, "The new control should be contained in the control list.");
        }
    }
}
