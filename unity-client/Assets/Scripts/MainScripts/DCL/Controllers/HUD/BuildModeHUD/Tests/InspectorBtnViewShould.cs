using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class InspectorBtnViewShould
    {
        private InspectorBtnView inspectorBtnView;

        [SetUp]
        public void SetUp()
        {
            inspectorBtnView = InspectorBtnView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(inspectorBtnView.gameObject);
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void ConfigureEventTriggerCorrectly(EventTriggerType eventType)
        {
            // Arrange
            bool triggerActionCalled = false;
            if (inspectorBtnView.inspectorButtonEventTrigger != null)
                inspectorBtnView.inspectorButtonEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);

            // Act
            inspectorBtnView.ConfigureEventTrigger(eventType, (eventData) =>
            {
                triggerActionCalled = true;
            });
            inspectorBtnView.inspectorButtonEventTrigger.triggers.First(x => x.eventID == eventType).callback.Invoke(null);

            // Assert
            Assert.IsTrue(
                inspectorBtnView.inspectorButtonEventTrigger.triggers.Count(x => x.eventID == eventType) == 1,
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
            inspectorBtnView.inspectorButtonEventTrigger.triggers.Add(newTrigger);

            // Act
            inspectorBtnView.RemoveEventTrigger(eventType);

            // Assert
            Assert.IsTrue(
                inspectorBtnView.inspectorButtonEventTrigger.triggers.Count(x => x.eventID == eventType) == 0,
                "The number of configured event triggers does not match!");
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            inspectorBtnView.OnInspectorButtonClick += () => isClicked = true;

            // Act
            inspectorBtnView.OnPointerClick(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            inspectorBtnView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            inspectorBtnView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            inspectorBtnView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The tooltip text does not match!");
            Assert.AreEqual(inspectorBtnView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            inspectorBtnView.OnHideTooltip += () => isHidden = true;

            // Act
            inspectorBtnView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }
    }
}
