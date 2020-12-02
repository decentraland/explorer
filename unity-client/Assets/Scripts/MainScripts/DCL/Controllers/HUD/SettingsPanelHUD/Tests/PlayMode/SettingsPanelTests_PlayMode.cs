using DCL.SettingsPanelHUD;
using DCL.SettingsPanelHUD.Sections;
using NSubstitute;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SettingsPanelTests_PlayMode : TestsBase
    {
        private const string SECTION_VIEW_PREFAB_PATH = "Sections/DefaultSettingsSectionTemplate";
        private const string MENU_BUTTON_PREFAB_PATH = "Sections/DefaultSettingsMenuButtonTemplate";

        private SettingsPanelHUDView panelView;
        private IHUD hudController;
        private ISettingsPanelHUDController panelController;
        private SettingsSectionList sectionsToCreate = new SettingsSectionList();

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            panelView = SettingsPanelHUDView.Create();
            hudController = Substitute.For<IHUD>();
            panelController = Substitute.For<ISettingsPanelHUDController>();
        }

        protected override IEnumerator TearDown()
        {
            Object.Destroy(panelView.gameObject);
            sectionsToCreate.Clear();

            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator GenerateSectionsIntoThePanelViewCorrectly()
        {
            // Arrange
            SettingsSectionView sectionViewPrefab = ((GameObject)Resources.Load(SECTION_VIEW_PREFAB_PATH)).GetComponent<SettingsSectionView>();
            SettingsButtonEntry menuButtonPrefab = ((GameObject)Resources.Load(MENU_BUTTON_PREFAB_PATH)).GetComponent<SettingsButtonEntry>();

            SettingsSectionModel newSectionConfig = new SettingsSectionModel(
                Sprite.Create(new Texture2D(10, 10), new Rect(), new Vector2()),
                $"TestSection",
                menuButtonPrefab,
                sectionViewPrefab,
                new SettingsSectionController(),
                new SettingsWidgetList());

            sectionsToCreate.Add(newSectionConfig);

            // Act
            panelView.Initialize(hudController, panelController, sectionsToCreate);
            yield return null;

            // Assert
            panelController.Received(1).AddSection(
                Arg.Any<SettingsButtonEntry>(),
                Arg.Any<ISettingsSectionView>(),
                Arg.Any<ISettingsSectionController>(),
                Arg.Any<SettingsSectionModel>());

            panelController.Received(1).OpenSection(0);
        }
    }
}
