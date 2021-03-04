using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class PublishBtnViewShould
    {
        private PublishBtnView publishBtnView;

        [SetUp]
        public void SetUp()
        {
            publishBtnView = PublishBtnView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(publishBtnView.gameObject);
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void ConfigureEventTriggerCorrectly(EventTriggerType eventType)
        {
            // Arrange
            bool triggerActionCalled = false;
            if (publishBtnView.publishButtonEventTrigger != null)
                publishBtnView.publishButtonEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);

            // Act
            publishBtnView.ConfigureEventTrigger(eventType, (eventData) =>
            {
                triggerActionCalled = true;
            });
            publishBtnView.publishButtonEventTrigger.triggers.First(x => x.eventID == eventType).callback.Invoke(null);

            // Assert
            Assert.IsTrue(
                publishBtnView.publishButtonEventTrigger.triggers.Count(x => x.eventID == eventType) == 1,
                "The number of configured event triggers does not match!");
            Assert.IsTrue(triggerActionCalled, "The trigger action has not been called!");
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void RemoveEventTriggerCorrectly(EventTriggerType eventType)
        {
            // Arrange
            EventTrigger.Entry newTrigger = new EventTrigger.Entry();
            newTrigger.eventID = eventType;
            newTrigger.callback.AddListener(null);
            publishBtnView.publishButtonEventTrigger.triggers.Add(newTrigger);

            // Act
            publishBtnView.RemoveEventTrigger(eventType);

            // Assert
            Assert.IsTrue(
                publishBtnView.publishButtonEventTrigger.triggers.Count(x => x.eventID == eventType) == 0,
                "The number of configured event triggers does not match!");
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            publishBtnView.OnPublishButtonClick += () => isClicked = true;

            // Act
            publishBtnView.OnPointerClick();

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            publishBtnView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            publishBtnView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            publishBtnView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The tooltip text does not match!");
            Assert.AreEqual(publishBtnView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            publishBtnView.OnHideTooltip += () => isHidden = true;

            // Act
            publishBtnView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetInteractableCorrectly(bool isInteractable)
        {
            // Arrange
            publishBtnView.mainButton.interactable = !isInteractable;

            // Act
            publishBtnView.SetInteractable(isInteractable);

            // Assert
            Assert.AreEqual(isInteractable, publishBtnView.mainButton.interactable, "The interactable property does not match!");
        }
    }
}
