using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class FirstPersonModeViewShould
    {
        private FirstPersonModeView firstPersonModeView;

        [SetUp]
        public void SetUp()
        {
            firstPersonModeView = FirstPersonModeView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(firstPersonModeView.gameObject);
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void ConfigureEventTriggerCorrectly(EventTriggerType eventType)
        {
            // Arrange
            bool triggerActionCalled = false;
            if (firstPersonModeView.changeModeEventTrigger != null)
                firstPersonModeView.changeModeEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);

            // Act
            firstPersonModeView.ConfigureEventTrigger(eventType, (eventData) =>
            {
                triggerActionCalled = true;
            });
            firstPersonModeView.changeModeEventTrigger.triggers.First(x => x.eventID == eventType).callback.Invoke(null);

            // Assert
            Assert.IsTrue(
                firstPersonModeView.changeModeEventTrigger.triggers.Count(x => x.eventID == eventType) == 1,
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
            firstPersonModeView.changeModeEventTrigger.triggers.Add(newTrigger);

            // Act
            firstPersonModeView.RemoveEventTrigger(eventType);

            // Assert
            Assert.IsTrue(
                firstPersonModeView.changeModeEventTrigger.triggers.Count(x => x.eventID == eventType) == 0,
                "The number of configured event triggers does not match!");
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            firstPersonModeView.OnFirstPersonModeClick += () => isClicked = true;

            // Act
            firstPersonModeView.OnPointerClick();

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            firstPersonModeView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            firstPersonModeView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            firstPersonModeView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The event data does not match!");
            Assert.AreEqual(firstPersonModeView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            firstPersonModeView.OnHideTooltip += () => isHidden = true;

            // Act
            firstPersonModeView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }
    }
}
