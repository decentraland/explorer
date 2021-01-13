using DCL.SettingsController;
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
            var newControlController = ScriptableObject.CreateInstance<SettingsControlController>();
            var newControlConfig = ScriptableObject.CreateInstance<SettingsControlModel>();
            newControlConfig.title = "TestControl";
            newControlConfig.controlPrefab = new GameObject().AddComponent<SettingsControlView>();
            newControlConfig.controlController = ScriptableObject.CreateInstance<SettingsControlController>();
            newControlConfig.flagsThatDeactivateMe = new List<BooleanVariable>();
            newControlConfig.flagsThatDisableMe = new List<BooleanVariable>();
            newControlConfig.isBeta = false;

            SettingsWidgetController widgetController = ScriptableObject.CreateInstance<SettingsWidgetController>();

            // Act
            widgetController.AddControl(
                newControlView,
                newControlController,
                newControlConfig,
                Substitute.For<IGeneralSettingsController>(),
                Substitute.For<IQualitySettingsController>());

            // Assert
            newControlView.Received(1).Initialize(
                newControlConfig,
                newControlController,
                Arg.Any<IGeneralSettingsController>(),
                Arg.Any<IQualitySettingsController>());

            Assert.Contains(newControlView, widgetController.controls, "The new control should be contained in the control list.");
        }
    }
}
