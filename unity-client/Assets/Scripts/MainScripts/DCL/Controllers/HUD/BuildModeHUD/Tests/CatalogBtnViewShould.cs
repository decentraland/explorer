using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class CatalogBtnViewShould
    {
        private CatalogBtnView catalogBtnView;

        [SetUp]
        public void SetUp()
        {
            catalogBtnView = CatalogBtnView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(catalogBtnView.gameObject);
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void ConfigureEventTriggerCorrectly(EventTriggerType eventType)
        {
            // Arrange
            bool triggerActionCalled = false;
            if (catalogBtnView.catalogButtonEventTrigger != null)
                catalogBtnView.catalogButtonEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);

            // Act
            catalogBtnView.ConfigureEventTrigger(eventType, (eventData) =>
            {
                triggerActionCalled = true;
            });
            catalogBtnView.catalogButtonEventTrigger.triggers.First(x => x.eventID == eventType).callback.Invoke(null);

            // Assert
            Assert.IsTrue(
                catalogBtnView.catalogButtonEventTrigger.triggers.Count(x => x.eventID == eventType) == 1,
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
            catalogBtnView.catalogButtonEventTrigger.triggers.Add(newTrigger);

            // Act
            catalogBtnView.RemoveEventTrigger(eventType);

            // Assert
            Assert.IsTrue(
                catalogBtnView.catalogButtonEventTrigger.triggers.Count(x => x.eventID == eventType) == 0,
                "The number of configured event triggers does not match!");
        }

        [Test]
        public void OnPointerClickCorrectly()
        {
            // Arrange
            bool isClicked = false;
            catalogBtnView.OnCatalogButtonClick += () => isClicked = true;

            // Act
            catalogBtnView.OnPointerClick();

            // Assert
            Assert.IsTrue(isClicked, "isClicked is false!");
        }

        [Test]
        public void OnPointerEnterCorrectly()
        {
            // Arrange
            PointerEventData sentEventData = new PointerEventData(null);
            catalogBtnView.tooltipText = "Test text";
            PointerEventData returnedEventData = null;
            string returnedTooltipText = "";
            catalogBtnView.OnShowTooltip += (data, text) =>
            {
                returnedEventData = (PointerEventData)data;
                returnedTooltipText = text;
            };

            // Act
            catalogBtnView.OnPointerEnter(sentEventData);

            // Assert
            Assert.AreEqual(sentEventData, returnedEventData, "The tooltip text does not match!");
            Assert.AreEqual(catalogBtnView.tooltipText, returnedTooltipText, "The tooltip text does not match!");
        }

        [Test]
        public void OnPointerExitCorrectly()
        {
            // Arrange
            bool isHidden = false;
            catalogBtnView.OnHideTooltip += () => isHidden = true;

            // Act
            catalogBtnView.OnPointerExit();

            // Assert
            Assert.IsTrue(isHidden, "isHidden is false!");
        }
    }
}
