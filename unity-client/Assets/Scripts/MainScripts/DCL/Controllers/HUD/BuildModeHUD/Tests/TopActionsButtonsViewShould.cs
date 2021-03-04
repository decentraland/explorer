using NSubstitute;
using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tests.BuildModeHUDViews
{
    public class TopActionsButtonsViewShould
    {
        private TopActionsButtonsView topActionsButtonsView;

        [SetUp]
        public void SetUp()
        {
            topActionsButtonsView = TopActionsButtonsView.Create();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(topActionsButtonsView.gameObject);
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void ConfigureAllEventTriggersCorrectly(EventTriggerType eventType)
        {
            TestEventTriggerConfiguration(topActionsButtonsView.changeCameraModeEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.translateEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.rotateEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.scaleEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.resetEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.duplicateEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.deleteEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.moreActionsEventTrigger, eventType);
            TestEventTriggerConfiguration(topActionsButtonsView.logoutEventTrigger, eventType);
        }

        private void TestEventTriggerConfiguration(EventTrigger eventTrigger, EventTriggerType eventType)
        {
            // Arrange
            bool triggerActionCalled = false;
            if (eventTrigger != null)
                eventTrigger.triggers.RemoveAll(x => x.eventID == eventType);

            // Act
            topActionsButtonsView.ConfigureEventTrigger(eventTrigger, eventType, (eventData) =>
            {
                triggerActionCalled = true;
            });
            eventTrigger.triggers.First(x => x.eventID == eventType).callback.Invoke(null);

            // Assert
            Assert.IsTrue(
                eventTrigger.triggers.Count(x => x.eventID == eventType) == 1,
                "The number of configured event triggers does not match!");
            Assert.IsTrue(triggerActionCalled, "The trigger action has not been called!");
        }

        [Test]
        [TestCase(EventTriggerType.PointerEnter)]
        [TestCase(EventTriggerType.PointerExit)]
        public void RemoveAllEventTriggersCorrectly(EventTriggerType eventType)
        {
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.changeCameraModeEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.translateEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.rotateEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.scaleEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.resetEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.duplicateEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.deleteEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.moreActionsEventTrigger, eventType);
            TestEventTriggerRemovingCorrectly(topActionsButtonsView.logoutEventTrigger, eventType);
        }

        private void TestEventTriggerRemovingCorrectly(EventTrigger eventTrigger, EventTriggerType eventType)
        {
            // Arrange
            EventTrigger.Entry newTrigger = new EventTrigger.Entry();
            newTrigger.eventID = eventType;
            newTrigger.callback.AddListener(null);
            eventTrigger.triggers.Add(newTrigger);

            // Act
            topActionsButtonsView.RemoveEventTrigger(eventTrigger, eventType);

            // Assert
            Assert.IsTrue(
                eventTrigger.triggers.Count(x => x.eventID == eventType) == 0,
                "The number of configured event triggers does not match!");
        }

        [Test]
        public void ConfigureExtraActionsCorrectly()
        {
            // Arrange
            IExtraActionsController mockedExtraActionsController = Substitute.For<IExtraActionsController>();
            topActionsButtonsView.extraActionsController = null;


            // Act
            topActionsButtonsView.ConfigureExtraActions(mockedExtraActionsController);

            // Assert
            Assert.AreEqual(mockedExtraActionsController, topActionsButtonsView.extraActionsController, "The extra actions controller does not match!");
            mockedExtraActionsController.Received(1).Initialize(topActionsButtonsView.extraActionsView);
        }

        [Test]
        public void ClickOnChangeModeCorrectly()
        {
            // Arrange
            bool modeIsChanged = false;
            topActionsButtonsView.OnChangeModeClicked += () => modeIsChanged = true;

            // Act
            topActionsButtonsView.OnChangeModeClick();

            // Assert
            Assert.IsTrue(modeIsChanged, "modeIsChanged is false!");
        }

        [Test]
        public void ClickOnExtraCorrectly()
        {
            // Arrange
            bool extraClicked = false;
            topActionsButtonsView.OnExtraClicked += () => extraClicked = true;

            // Act
            topActionsButtonsView.OnExtraClick();

            // Assert
            Assert.IsTrue(extraClicked, "extraClicked is false!");
        }

        [Test]
        public void ClickOnTranslateCorrectly()
        {
            // Arrange
            bool translateClicked = false;
            topActionsButtonsView.OnTranslateClicked += () => translateClicked = true;

            // Act
            topActionsButtonsView.OnTranslateClick();

            // Assert
            Assert.IsTrue(translateClicked, "translateClicked is false!");
        }

        [Test]
        public void ClickOnRotateCorrectly()
        {
            // Arrange
            bool rotateClicked = false;
            topActionsButtonsView.OnRotateClicked += () => rotateClicked = true;

            // Act
            topActionsButtonsView.OnRotateClick();

            // Assert
            Assert.IsTrue(rotateClicked, "rotateClicked is false!");
        }

        [Test]
        public void ClickOnScaleCorrectly()
        {
            // Arrange
            bool scaleClicked = false;
            topActionsButtonsView.OnScaleClicked += () => scaleClicked = true;

            // Act
            topActionsButtonsView.OnScaleClick();

            // Assert
            Assert.IsTrue(scaleClicked, "scaleClicked is false!");
        }

        [Test]
        public void ClickOnResetCorrectly()
        {
            // Arrange
            bool resetClicked = false;
            topActionsButtonsView.OnResetClicked += () => resetClicked = true;

            // Act
            topActionsButtonsView.OnResetClick();

            // Assert
            Assert.IsTrue(resetClicked, "resetClicked is false!");
        }

        [Test]
        public void ClickOnDuplicateCorrectly()
        {
            // Arrange
            bool duplicateClicked = false;
            topActionsButtonsView.OnDuplicateClicked += () => duplicateClicked = true;

            // Act
            topActionsButtonsView.OnDuplicateClick();

            // Assert
            Assert.IsTrue(duplicateClicked, "duplicateClicked is false!");
        }

        [Test]
        public void ClickOnDeleteCorrectly()
        {
            // Arrange
            bool deleteClicked = false;
            topActionsButtonsView.OnDeleteClicked += () => deleteClicked = true;

            // Act
            topActionsButtonsView.OnDeleteClick();

            // Assert
            Assert.IsTrue(deleteClicked, "deleteClicked is false!");
        }

        [Test]
        public void ClickOnLogoutCorrectly()
        {
            // Arrange
            bool logoutClicked = false;
            topActionsButtonsView.OnLogOutClicked += () => logoutClicked = true;

            // Act
            topActionsButtonsView.OnLogOutClick();

            // Assert
            Assert.IsTrue(logoutClicked, "logoutClicked is false!");
        }
    }
}
