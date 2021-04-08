using NSubstitute;
using NUnit.Framework;

namespace Tests.BuildModeHUDControllers
{
    public class ExitFromBuildModeControllerShould
    {
        private ExitFromBuildModeController exitFromBiWModalController;

        [SetUp]
        public void SetUp()
        {
            exitFromBiWModalController = new ExitFromBuildModeController();
            exitFromBiWModalController.Initialize(Substitute.For<IExitFromBuildModeView>());
        }

        [TearDown]
        public void TearDown() { exitFromBiWModalController.Dispose(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Act
            exitFromBiWModalController.SetActive(isActive);

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(isActive);
        }

        [Test]
        public void CancelExitCorrectly()
        {
            // Arrange
            bool canceled = false;
            exitFromBiWModalController.OnCancelExit += () => { canceled = true; };

            // Act
            exitFromBiWModalController.CancelExit();

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(false);
            Assert.IsTrue(canceled, "The canceled flag is false!");
        }

        [Test]
        public void ConfirmExitCorrectly()
        {
            // Arrange
            bool confirmed = false;
            exitFromBiWModalController.OnConfirmExit += () => { confirmed = true; };

            // Act
            exitFromBiWModalController.ConfirmExit();

            // Assert
            exitFromBiWModalController.exitFromBiWModalView.Received(1).SetActive(false);
            Assert.IsTrue(confirmed, "The confirmed flag is false!");
        }
    }
}