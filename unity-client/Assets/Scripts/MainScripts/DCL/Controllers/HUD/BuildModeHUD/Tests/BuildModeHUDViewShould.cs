using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class BuildModeHUDViewShould
    {
        private BuildModeHUDView buildModeHUDView;
        ITooltipController tooltipController;
        ISceneCatalogController sceneCatalogController;
        IQuickBarController quickBarController;
        IEntityInformationController entityInformationController;
        IFirstPersonModeController firstPersonModeController;
        IShortcutsController shortcutsController;
        IPublishPopupController publishPopupController;
        IDragAndDropSceneObjectController dragAndDropSceneObjectController;
        IPublishBtnController publishBtnController;
        IInspectorBtnController inspectorBtnController;
        ICatalogBtnController catalogBtnController;
        IInspectorController inspectorController;
        ITopActionsButtonsController topActionsButtonsController;

        [SetUp]
        public void SetUp()
        {
            buildModeHUDView = BuildModeHUDView.Create();
            tooltipController = Substitute.For<ITooltipController>();
            sceneCatalogController = Substitute.For<ISceneCatalogController>();
            quickBarController = Substitute.For<IQuickBarController>();
            entityInformationController = Substitute.For<IEntityInformationController>();
            firstPersonModeController = Substitute.For<IFirstPersonModeController>();
            shortcutsController = Substitute.For<IShortcutsController>();
            publishPopupController = Substitute.For<IPublishPopupController>();
            dragAndDropSceneObjectController = Substitute.For<IDragAndDropSceneObjectController>();
            publishBtnController = Substitute.For<IPublishBtnController>();
            inspectorBtnController = Substitute.For<IInspectorBtnController>();
            catalogBtnController = Substitute.For<ICatalogBtnController>();
            inspectorController = Substitute.For<IInspectorController>();
            topActionsButtonsController = Substitute.For<ITopActionsButtonsController>();

            buildModeHUDView.Initialize(
                tooltipController,
                sceneCatalogController,
                quickBarController,
                entityInformationController,
                firstPersonModeController,
                shortcutsController,
                publishPopupController,
                dragAndDropSceneObjectController,
                publishBtnController,
                inspectorBtnController,
                catalogBtnController,
                inspectorController,
                topActionsButtonsController);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(buildModeHUDView.gameObject);
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.AreEqual(tooltipController, buildModeHUDView.tooltipController, "The tooltipController does not match!");
            tooltipController.Received(1).Initialize(buildModeHUDView.tooltipView);
            Assert.AreEqual(sceneCatalogController, buildModeHUDView.sceneCatalogController, "The sceneCatalogController does not match!");
            sceneCatalogController.Received(1).Initialize(buildModeHUDView.sceneCatalogView, quickBarController);
            Assert.AreEqual(quickBarController, buildModeHUDView.quickBarController, "The quickBarController does not match!");
            quickBarController.Received(1).Initialize(buildModeHUDView.quickBarView, sceneCatalogController);
            Assert.AreEqual(entityInformationController, buildModeHUDView.entityInformationController, "The entityInformationController does not match!");
            entityInformationController.Received(1).Initialize(buildModeHUDView.entityInformationView);
            Assert.AreEqual(firstPersonModeController, buildModeHUDView.firstPersonModeController, "The firstPersonModeController does not match!");
            firstPersonModeController.Received(1).Initialize(buildModeHUDView.firstPersonModeView, tooltipController);
            Assert.AreEqual(shortcutsController, buildModeHUDView.shortcutsController, "The shortcutsController does not match!");
            shortcutsController.Received(1).Initialize(buildModeHUDView.shortcutsView);
            Assert.AreEqual(publishPopupController, buildModeHUDView.publishPopupController, "The publishPopupController does not match!");
            publishPopupController.Received(1).Initialize(buildModeHUDView.publishPopupView);
            Assert.AreEqual(dragAndDropSceneObjectController, buildModeHUDView.dragAndDropSceneObjectController, "The dragAndDropSceneObjectController does not match!");
            dragAndDropSceneObjectController.Received(1).Initialize(buildModeHUDView.dragAndDropSceneObjectView);
            Assert.AreEqual(publishBtnController, buildModeHUDView.publishBtnController, "The publishBtnController does not match!");
            publishBtnController.Received(1).Initialize(buildModeHUDView.publishBtnView, tooltipController);
            Assert.AreEqual(inspectorBtnController, buildModeHUDView.inspectorBtnController, "The inspectorBtnController does not match!");
            inspectorBtnController.Received(1).Initialize(buildModeHUDView.inspectorBtnView, tooltipController);
            Assert.AreEqual(catalogBtnController, buildModeHUDView.catalogBtnController, "The catalogBtnController does not match!");
            catalogBtnController.Received(1).Initialize(buildModeHUDView.catalogBtnView, tooltipController);
            Assert.AreEqual(inspectorController, buildModeHUDView.inspectorController, "The inspectorController does not match!");
            inspectorController.Received(1).Initialize(buildModeHUDView.inspectorView);
            Assert.AreEqual(topActionsButtonsController, buildModeHUDView.topActionsButtonsController, "The topActionsButtonsController does not match!");
            topActionsButtonsController.Received(1).Initialize(buildModeHUDView.topActionsButtonsView, tooltipController);
        }

        [Test]
        public void PublishStartCorrectly()
        {
            // Act
            buildModeHUDView.PublishStart();

            // Assert
            publishPopupController.Received(1).PublishStart();
        }

        [Test]
        public void PublishEndCorrectly()
        {
            // Arrange
            string testText = "Test text";

            // Act
            buildModeHUDView.PublishEnd(testText);

            // Assert
            publishPopupController.Received(1).PublishEnd(testText);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetPublishBtnAvailabilityCorrectly(bool isAvailable)
        {
            // Act
            buildModeHUDView.SetPublishBtnAvailability(isAvailable);

            // Assert
            publishBtnController.Received(1).SetInteractable(isAvailable);
        }

        [Test]
        public void RefreshCatalogAssetPackCorrectly()
        {
            // Act
            buildModeHUDView.RefreshCatalogAssetPack();

            // Assert
            sceneCatalogController.Received(1).RefreshAssetPack();
        }

        [Test]
        public void RefreshCatalogContentCorrectly()
        {
            // Act
            buildModeHUDView.RefreshCatalogContent();

            // Assert
            sceneCatalogController.Received(1).RefreshCatalog();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfCatalogCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfCatalog(isVisible);

            // Assert
            if (isVisible)
                sceneCatalogController.Received(1).OpenCatalog();
            else
                sceneCatalogController.Received(1).CloseCatalog();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfSceneInfoCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfSceneInfo(isVisible);

            // Assert
            if (isVisible)
                inspectorController.sceneLimitsController.Received(1).Enable();
            else
                inspectorController.sceneLimitsController.Received(1).Disable();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfControlsCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfControls(isVisible);

            // Assert
            shortcutsController.Received(1).SetActive(isVisible);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityOfExtraBtnsCorrectly(bool isVisible)
        {
            // Act
            buildModeHUDView.SetVisibilityOfExtraBtns(isVisible);

            // Assert
            topActionsButtonsController.extraActionsController.Received(1).SetActive(isVisible);
        }

        [Test]
        public void SetFirstPersonViewCorrectly()
        {
            // Arrange
            buildModeHUDView.firstPersonCanvasGO.SetActive(false);
            buildModeHUDView.godModeCanvasGO.SetActive(true);

            // Act
            buildModeHUDView.SetFirstPersonView();

            // Assert
            Assert.IsTrue(buildModeHUDView.firstPersonCanvasGO.activeSelf, "The firstPersonCanvasGO active property is false!");
            Assert.IsFalse(buildModeHUDView.godModeCanvasGO.activeSelf, "The godModeCanvasGO active property is true!");
        }

        [Test]
        public void HideToolTipCorrectly()
        {
            // Act
            buildModeHUDView.HideToolTip();

            // Assert
            tooltipController.Received(1).HideTooltip();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            buildModeHUDView.gameObject.SetActive(!isActive);

            // Act
            buildModeHUDView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, buildModeHUDView.gameObject.activeSelf, "The game object actove property does not match!");
        }
    }
}
