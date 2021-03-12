using DCL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class QuickBarViewShould
    {
        private QuickBarView quickBarView;

        [SetUp]
        public void SetUp() { quickBarView = QuickBarView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(quickBarView.gameObject); }

        [Test]
        public void SelectQuickBarObjectCorrectly()
        {
            // Arrange
            int selectedObjectIndex = -1;
            int indexToSelect = 3;
            quickBarView.OnQuickBarObjectSelected += (index) => selectedObjectIndex = index;

            // Act
            quickBarView.QuickBarObjectSelected(indexToSelect);

            // Assert
            Assert.AreEqual(indexToSelect, selectedObjectIndex, "The selected object index does not match!");
        }

        [Test]
        public void SetIndexToBeginDragCorrectly()
        {
            // Arrange
            int draggedObjectIndex = -1;
            int indexToBeginDrag = 3;
            quickBarView.OnSetIndexToBeginDrag += (index) => draggedObjectIndex = index;

            // Act
            quickBarView.SetIndexToBeginDrag(indexToBeginDrag);

            // Assert
            Assert.AreEqual(indexToBeginDrag, draggedObjectIndex, "The dragged object index does not match!");
        }

        [Test]
        public void SetIndexToDropCorrectly()
        {
            // Arrange
            int dropObjectIndex = -1;
            int indexToDrop = 3;
            quickBarView.OnSetIndexToDrop += (index) => dropObjectIndex = index;

            // Act
            quickBarView.SetIndexToDrop(indexToDrop);

            // Assert
            Assert.AreEqual(indexToDrop, dropObjectIndex, "The drop object index does not match!");
        }

        [Test]
        public void DropSceneObjectFromQuickBarCorrectly()
        {
            // Arrange
            Texture testTexture = null;
            int testFromIndex = 0;
            int testToIndex = 1;
            Texture returnedTexture;
            int returnedFromIndex = 0;
            int returnedToIndex = 1;

            quickBarView.OnSceneObjectDroppedFromQuickBar += (fromIndex, toIndex, texture) =>
            {
                returnedFromIndex = fromIndex;
                returnedToIndex = toIndex;
                returnedTexture = texture;
            };

            // Act
            quickBarView.SceneObjectDroppedFromQuickBar(testFromIndex, testToIndex, testTexture);

            // Assert
            Assert.AreEqual(returnedFromIndex, testFromIndex, "The returnedFromIndex does not match!");
            Assert.AreEqual(returnedToIndex, testToIndex, "The returnedToIndex does not match!");
        }

        [Test]
        public void DropSceneObjectFromCatalogCorrectly()
        {
            // Arrange
            BaseEventData droppedObject = null;
            BaseEventData objectToDrop = new BaseEventData(null);
            quickBarView.OnSceneObjectDroppedFromCatalog += (data) => droppedObject = data;

            // Act
            quickBarView.SceneObjectDroppedFromCatalog(objectToDrop);

            // Assert
            Assert.IsNotNull(droppedObject, "The dropped object is null!");
            Assert.AreEqual(objectToDrop, droppedObject, "The dropped object does not match!");
        }

        [Test]
        public void TriggerQuickBarInputCorrectly()
        {
            // Arrange
            int triggeredIndex = -1;
            int indexToDrop = 3;
            quickBarView.OnQuickBarInputTriggered += (index) => triggeredIndex = index;

            // Act
            quickBarView.OnQuickBarInputTriggedered(indexToDrop);

            // Assert
            Assert.AreEqual(indexToDrop, triggeredIndex, "The triggered index does not match!");
        }

        [Test]
        public void CancelCurrentDraggingCorrectly()
        {
            // Arrange
            quickBarView.lastIndexToBeginDrag = 5;

            // Act
            quickBarView.CancelCurrentDragging();

            // Assert
            Assert.AreEqual(-1, quickBarView.lastIndexToBeginDrag, "The lastIndexToBeginDrag does not match!");
        }
    }
}