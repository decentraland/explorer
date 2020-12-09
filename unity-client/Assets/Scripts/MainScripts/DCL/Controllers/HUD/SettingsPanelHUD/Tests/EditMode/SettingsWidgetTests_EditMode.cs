using DCL.SettingsPanelHUD.Controls;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace SettingsWidgetTests
{

    public class SettingsWidgetShould_EditMode
    {
		[Test]
        public void AddControlCorrectly()
        {
            // Arrange
            var newControlView = Substitute.For<ISettingsControlView>();
            var newControlController = Substitute.For<SettingsControlController>();
            var newControlConfig = ScriptableObject.CreateInstance<SettingsControlModel>();
            newControlConfig.title = "TestControl";
            newControlConfig.controlPrefab = Substitute.For<SettingsControlView>();
            newControlConfig.controlController = Substitute.For<SettingsControlController>();
            newControlConfig.flagsThatDeactivatesMe = new List<BooleanVariable>();
            newControlConfig.flagsThatDisablesMe = new List<BooleanVariable>();
            newControlConfig.isBeta = false;

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
