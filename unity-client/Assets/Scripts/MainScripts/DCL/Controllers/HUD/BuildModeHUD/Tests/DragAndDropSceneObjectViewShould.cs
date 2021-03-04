using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class DragAndDropSceneObjectViewShould
    {
        private DragAndDropSceneObjectView dragAndDropSceneObjectView;

        [SetUp]
        public void SetUp()
        {
            dragAndDropSceneObjectView = DragAndDropSceneObjectView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(dragAndDropSceneObjectView.gameObject);
        }

        [Test]
        public void ConfigureEventTriggerCorrectly()
        {
            // Arrange
            bool triggerActionCalled = false;
            if (dragAndDropSceneObjectView.dragAndDropEventTrigger != null)
                dragAndDropSceneObjectView.dragAndDropEventTrigger.triggers.RemoveAll(x => x.eventID == EventTriggerType.Drop);

            // Act
            dragAndDropSceneObjectView.ConfigureEventTrigger(EventTriggerType.Drop, (eventData) =>
            {
                triggerActionCalled = true;
            });
            dragAndDropSceneObjectView.dragAndDropEventTrigger.triggers.First(x => x.eventID == EventTriggerType.Drop).callback.Invoke(null);

            // Assert
            Assert.IsTrue(
                dragAndDropSceneObjectView.dragAndDropEventTrigger.triggers.Count(x => x.eventID == EventTriggerType.Drop) == 1,
                "The number of configured event triggers does not match!");
            Assert.IsTrue(triggerActionCalled, "The trigger action has not been called!");
        }

        [Test]
        public void RemoveEventTriggerCorrectly()
        {
            // Arrange
            EventTrigger.Entry newTrigger = new EventTrigger.Entry();
            newTrigger.eventID = EventTriggerType.Drop;
            newTrigger.callback.AddListener(null);
            dragAndDropSceneObjectView.dragAndDropEventTrigger.triggers.Add(newTrigger);

            // Act
            dragAndDropSceneObjectView.RemoveEventTrigger(EventTriggerType.Drop);

            // Assert
            Assert.IsTrue(
                dragAndDropSceneObjectView.dragAndDropEventTrigger.triggers.Count(x => x.eventID == EventTriggerType.Drop) == 0,
                "The number of configured event triggers does not match!");
        }

        [Test]
        public void DropCorrectly()
        {
            // Arrange
            bool isDropped = false;
            dragAndDropSceneObjectView.OnDrop += () => isDropped = true;

            // Act
            dragAndDropSceneObjectView.Drop();

            // Assert
            Assert.IsTrue(isDropped, "isDropped is false!");
        }
    }
}
