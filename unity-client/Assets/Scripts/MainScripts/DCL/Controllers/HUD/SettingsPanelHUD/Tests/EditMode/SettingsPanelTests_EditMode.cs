using DCL.SettingsPanelHUD;
using DCL.SettingsPanelHUD.Sections;
using DCL.SettingsPanelHUD.Widgets;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class SettingsPanelTests_EditMode
    {
        private SettingsPanelHUDController panelController;
        private ISettingsSectionView newSectionView;
        private ISettingsSectionController newSectionController;
        private SettingsSectionModel newSectionConfig;

        [SetUp]
        public void SetUp()
        {
            panelController = new SettingsPanelHUDController();
            newSectionView = Substitute.For<ISettingsSectionView>();
            newSectionController = Substitute.For<ISettingsSectionController>();
            newSectionConfig = new SettingsSectionModel(
                Sprite.Create(new Texture2D(10, 10), new Rect(), new Vector2()),
                "TestSection",
                new SettingsButtonEntry(),
                new SettingsSectionView(),
                new SettingsSectionController(),
                new SettingsWidgetList());
        }

        [TearDown]
        public void TearDown()
        {
            panelController.sections.Clear();
        }

        [Test]
        public void AddSectionCorrectly()
        {
            // Act
            panelController.AddSection(null, newSectionView, newSectionController, newSectionConfig);

            // Assert
            newSectionView.Received(1).Initialize(newSectionController, Arg.Any<List<SettingsWidgetModel>>());
            newSectionView.Received(1).SetActive(false);
            Assert.Contains(newSectionView, panelController.sections, "The new section should be contained in the section list.");
        }

        [Test]
        public void OpenSectionCorrectly()
        {
            //Arrange
            panelController.AddSection(null, newSectionView, newSectionController, newSectionConfig);

            // Act
            panelController.OpenSection(newSectionView);

            // Assert
            newSectionView.Received(1).SetActive(true);
        }

        [Test]
        public void OpenSectionByIndexCorrectly()
        {
            //Arrange
            panelController.AddSection(null, newSectionView, newSectionController, newSectionConfig);

            // Act
            panelController.OpenSection(panelController.sections.Count - 1);

            // Assert
            newSectionView.Received(1).SetActive(true);
        }
    }
}