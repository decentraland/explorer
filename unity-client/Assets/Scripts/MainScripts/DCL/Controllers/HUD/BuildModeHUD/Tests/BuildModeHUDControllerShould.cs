using DCL.Controllers;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.BuildModeHUDControllers
{
    public class BuildModeHUDControllerShould
    {
        private BuildModeHUDController buildModeHUDController;

        [SetUp]
        public void SetUp()
        {
            buildModeHUDController = new BuildModeHUDController(
                Substitute.For<IBuildModeHUDView>(),
                Substitute.For<ITooltipController>(),
                Substitute.For<ISceneCatalogController>(),
                Substitute.For<IQuickBarController>(),
                Substitute.For<IEntityInformationController>(),
                Substitute.For<IFirstPersonModeController>(),
                Substitute.For<IShortcutsController>(),
                Substitute.For<IPublishPopupController>(),
                Substitute.For<IDragAndDropSceneObjectController>(),
                Substitute.For<IPublishBtnController>(),
                Substitute.For<IInspectorBtnController>(),
                Substitute.For<ICatalogBtnController>(),
                Substitute.For<IInspectorController>(),
                Substitute.For<ITopActionsButtonsController>());
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CreateBuildModeControllersCorrectly()
        {
            // Arrange
            buildModeHUDController.tooltipController = null;
            buildModeHUDController.sceneCatalogController = null;
            buildModeHUDController.quickBarController = null;
            buildModeHUDController.entityInformationController = null;
            buildModeHUDController.firstPersonModeController = null;
            buildModeHUDController.shortcutsController = null;
            buildModeHUDController.publishPopupController = null;
            buildModeHUDController.dragAndDropSceneObjectController = null;
            buildModeHUDController.publishBtnController = null;
            buildModeHUDController.inspectorBtnController = null;
            buildModeHUDController.catalogBtnController = null;
            buildModeHUDController.inspectorController = null;
            buildModeHUDController.topActionsButtonsController = null;

            // Act
            buildModeHUDController.CreateBuildModeControllers();

            // Assert
            Assert.NotNull(buildModeHUDController.tooltipController, "The tooltipController is null!");
            Assert.NotNull(buildModeHUDController.sceneCatalogController, "The sceneCatalogController is null!");
            Assert.NotNull(buildModeHUDController.quickBarController, "The quickBarController is null!");
            Assert.NotNull(buildModeHUDController.entityInformationController, "The entityInformationController is null!");
            Assert.NotNull(buildModeHUDController.firstPersonModeController, "The firstPersonModeController is null!");
            Assert.NotNull(buildModeHUDController.shortcutsController, "The shortcutsController is null!");
            Assert.NotNull(buildModeHUDController.publishPopupController, "The publishPopupController is null!");
            Assert.NotNull(buildModeHUDController.dragAndDropSceneObjectController, "The dragAndDropSceneObjectController is null!");
            Assert.NotNull(buildModeHUDController.publishBtnController, "The publishBtnController is null!");
            Assert.NotNull(buildModeHUDController.inspectorBtnController, "The inspectorBtnController is null!");
            Assert.NotNull(buildModeHUDController.catalogBtnController, "The catalogBtnController is null!");
            Assert.NotNull(buildModeHUDController.inspectorController, "The inspectorController is null!");
            Assert.NotNull(buildModeHUDController.topActionsButtonsController, "The topActionsButtonsController is null!");
        }

        [Test]
        public void CreateParentViewCorrectly()
        {
            // Arrange
            buildModeHUDController.view = null;

            // Act
            buildModeHUDController.CreateParentView(Substitute.For<IBuildModeHUDView>());

            // Assert
            Assert.NotNull(buildModeHUDController.view, "The view is null!");
            buildModeHUDController.view.Received(1).Initialize(
                buildModeHUDController.tooltipController,
                buildModeHUDController.sceneCatalogController,
                buildModeHUDController.quickBarController,
                buildModeHUDController.entityInformationController,
                buildModeHUDController.firstPersonModeController,
                buildModeHUDController.shortcutsController,
                buildModeHUDController.publishPopupController,
                buildModeHUDController.dragAndDropSceneObjectController,
                buildModeHUDController.publishBtnController,
                buildModeHUDController.inspectorBtnController,
                buildModeHUDController.catalogBtnController,
                buildModeHUDController.inspectorController,
                buildModeHUDController.topActionsButtonsController);
        }

        [Test]
        public void PublishStartCorrectly()
        {
            // Act
            buildModeHUDController.PublishStart();

            // Assert
            buildModeHUDController.view.Received(1).PublishStart();
        }

        [Test]
        public void PublishEndCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            buildModeHUDController.PublishEnd(testText);

            // Assert
            buildModeHUDController.view.Received(1).PublishEnd(testText);
        }

        [Test]
        public void SetParcelSceneCorrectly()
        {
            // Arrange
            ParcelScene testParcelScene = new GameObject().AddComponent<ParcelScene>();

            // Act
            buildModeHUDController.SetParcelScene(testParcelScene);

            // Assert
            buildModeHUDController.inspectorController.sceneLimitsController.Received(1).SetParcelScene(testParcelScene);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishBtnAvailabilityCorrectly(bool isAvailable)
        {
            // Act
            buildModeHUDController.SetPublishBtnAvailability(isAvailable);

            // Assert
            buildModeHUDController.view.Received(1).SetPublishBtnAvailability(isAvailable);
        }

        [Test]
        public void RefreshCatalogAssetPackCorrectly()
        {
            // Act
            buildModeHUDController.RefreshCatalogAssetPack();

            // Assert
            buildModeHUDController.view.Received(1).RefreshCatalogAssetPack();
        }

        [Test]
        public void RefreshCatalogContentCorrectly()
        {
            // Act
            buildModeHUDController.RefreshCatalogContent();

            // Assert
            buildModeHUDController.view.Received(1).RefreshCatalogContent();
        }

        [Test]
        public void CatalogItemSelectedCorrectly()
        {
            // Arrange
            CatalogItem returnedCatalogItem = null;
            CatalogItem testCatalogItem = new CatalogItem();
            buildModeHUDController.OnCatalogItemSelected += (item) => { returnedCatalogItem = item; };

            // Act
            buildModeHUDController.CatalogItemSelected(testCatalogItem);

            // Assert
            Assert.AreEqual(testCatalogItem, returnedCatalogItem, "The catalog item does not march!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfCatalogCorrectly(bool isVisible)
        {
            // Arrange
            bool catalogOpened = false;
            buildModeHUDController.isCatalogOpen = !isVisible;
            buildModeHUDController.OnCatalogOpen += () => { catalogOpened = true; };

            // Act
            buildModeHUDController.SetVisibilityOfCatalog(isVisible);

            // Assert
            Assert.AreEqual(isVisible, buildModeHUDController.isCatalogOpen, "The isCatalogOpen does not match!");
            buildModeHUDController.view.Received(1).SetVisibilityOfCatalog(buildModeHUDController.isCatalogOpen);

            if (isVisible)
                Assert.IsTrue(catalogOpened, "catalogOpened is false!");
        }

        [Test]
        public void ChangeVisibilityOfCatalogCorrectly()
        {
            // Arrange
            buildModeHUDController.isCatalogOpen = buildModeHUDController.sceneCatalogController.IsCatalogOpen();

            // Act
            buildModeHUDController.ChangeVisibilityOfCatalog();

            // Assert
            Assert.AreEqual(
                !buildModeHUDController.sceneCatalogController.IsCatalogOpen(), 
                buildModeHUDController.isCatalogOpen, 
                "The isCatalogOpen does not match!");
        }

        [Test]
        public void UpdateSceneLimitInfoCorrectly()
        {
            // Act
            buildModeHUDController.UpdateSceneLimitInfo();

            // Assert
            buildModeHUDController.inspectorController.sceneLimitsController.Received(1).UpdateInfo();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeVisibilityOfSceneInfoCorrectly(bool shouldBeVisible)
        {
            // Arrange
            buildModeHUDController.isSceneLimitInfoVisibile = !shouldBeVisible;

            // Act
            buildModeHUDController.ChangeVisibilityOfSceneInfo(shouldBeVisible);

            // Assert
            Assert.AreEqual(shouldBeVisible, buildModeHUDController.isSceneLimitInfoVisibile, "The isSceneLimitInfoVisibile does not match!");
            buildModeHUDController.view.Received(1).SetVisibilityOfSceneInfo(buildModeHUDController.isSceneLimitInfoVisibile);
        }

        [Test]
        public void ChangeVisibilityOfSceneInfoCorrectly()
        {
            // Arrange
            buildModeHUDController.isSceneLimitInfoVisibile = false;

            // Act
            buildModeHUDController.ChangeVisibilityOfSceneInfo();

            // Assert
            Assert.IsTrue(buildModeHUDController.isSceneLimitInfoVisibile, "The isSceneLimitInfoVisibile is false!");
            buildModeHUDController.view.Received(1).SetVisibilityOfSceneInfo(buildModeHUDController.isSceneLimitInfoVisibile);
        }

        [Test]
        public void ActivateFirstPersonModeUICorrectly()
        {
            // Act
            buildModeHUDController.ActivateFirstPersonModeUI();

            // Assert
            buildModeHUDController.view.Received(1).SetFirstPersonView();
        }

        [Test]
        public void ActivateGodModeUICorrectly()
        {
            // Act
            buildModeHUDController.ActivateGodModeUI();

            // Assert
            buildModeHUDController.view.Received(1).SetGodModeView();
        }

        [Test]
        public void EntityInformationSetEntityCorrectly()
        {
            // Arrange
            DCLBuilderInWorldEntity testEntity = new GameObject().AddComponent<DCLBuilderInWorldEntity>();
            ParcelScene testScene = new GameObject().AddComponent<ParcelScene>();

            // Act
            buildModeHUDController.EntityInformationSetEntity(testEntity, testScene);

            // Assert
            buildModeHUDController.entityInformationController.Received(1).SetEntity(testEntity, testScene);
        }

        [Test]
        public void ShowEntityInformationCorrectly()
        {
            // Act
            buildModeHUDController.ShowEntityInformation();

            // Assert
            buildModeHUDController.entityInformationController.Received(1).Enable();
        }

        [Test]
        public void HideEntityInformationCorrectly()
        {
            // Act
            buildModeHUDController.HideEntityInformation();

            // Assert
            buildModeHUDController.entityInformationController.Received(1).Disable();
        }

        [Test]
        public void SetEntityListCorrectly()
        {
            // Arrange
            List<DCLBuilderInWorldEntity> testEntityList = new List<DCLBuilderInWorldEntity>();
            testEntityList.Add(new GameObject().AddComponent<DCLBuilderInWorldEntity>());
            testEntityList.Add(new GameObject().AddComponent<DCLBuilderInWorldEntity>());
            testEntityList.Add(new GameObject().AddComponent<DCLBuilderInWorldEntity>());

            // Act
            buildModeHUDController.SetEntityList(testEntityList);

            // Assert
            buildModeHUDController.inspectorController.Received(1).SetEntityList(testEntityList);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ChangeVisibilityOfEntityListCorrectly(bool isVisible)
        {
            // Arrange
            bool isEntityListVisible = false;
            buildModeHUDController.isEntityListVisible = isVisible;
            buildModeHUDController.OnEntityListVisible += () => { isEntityListVisible = true; };

            // Act
            buildModeHUDController.ChangeVisibilityOfEntityList();

            // Assert
            if (buildModeHUDController.isEntityListVisible)
            {
                Assert.IsTrue(isEntityListVisible, "isEntityListVisible is false!");
                buildModeHUDController.inspectorController.Received(1).OpenEntityList();
            }
            else
            {
                buildModeHUDController.inspectorController.Received(1).CloseList();
            }
        }

        [Test]
        public void ClearEntityListCorrectly()
        {
            // Act
            buildModeHUDController.ClearEntityList();

            // Assert
            buildModeHUDController.inspectorController.Received(1).ClearList();
        }

        [Test]
        public void ChangeVisibilityOfControlsCorrectly()
        {
            // Arrange
            buildModeHUDController.isControlsVisible = false;

            // Act
            buildModeHUDController.ChangeVisibilityOfControls();

            // Assert
            Assert.IsTrue(buildModeHUDController.isControlsVisible, "The isControlsVisible is false!");
            buildModeHUDController.view.Received(1).SetVisibilityOfControls(buildModeHUDController.isControlsVisible);
        }

        [Test]
        public void ChangeVisibilityOfExtraBtnsCorrectly()
        {
            // Arrange
            buildModeHUDController.areExtraButtonsVisible = false;

            // Act
            buildModeHUDController.ChangeVisibilityOfExtraBtns();

            // Assert
            Assert.IsTrue(buildModeHUDController.areExtraButtonsVisible, "The areExtraButtonsVisible is false!");
            buildModeHUDController.view.Received(1).SetVisibilityOfExtraBtns(buildModeHUDController.areExtraButtonsVisible);
        }
    }
}
