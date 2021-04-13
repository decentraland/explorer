using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class PublishPopupControllerShould
    {
        private PublishPopupController publishPopupController;

        [SetUp]
        public void SetUp()
        {
            publishPopupController = new PublishPopupController();
            publishPopupController.Initialize(Substitute.For<IPublishPopupView>());
        }

        [TearDown]
        public void TearDown() { publishPopupController.Dispose(); }

        [Test]
        public void PublishStartCorrectly()
        {
            // Act
            publishPopupController.PublishStart();

            // Assert
            publishPopupController.publishPopupView.Received(1).PublishStart();
            publishPopupController.publishPopupView.Received(1).SetPercentage(0f);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void PublishEndCorrectly(bool isOk)
        {
            // Act
            publishPopupController.PublishEnd(isOk);

            // Assert
            publishPopupController.publishPopupView.Received(1).PublishEnd(isOk);
        }

        [Test]
        public void SetPercentageCorrectly()
        {
            // Arrange
            float testPercentage = 16.8f;

            // Act
            publishPopupController.SetPercentage(testPercentage);

            // Assert
            publishPopupController.publishPopupView.Received(1).SetPercentage(testPercentage);
        }
    }
}