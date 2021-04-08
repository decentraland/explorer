using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class ExitFromBuildModeViewShould
    {
        private ExitFromBuildModeView exitFromBiWModalView;

        [SetUp]
        public void SetUp() { exitFromBiWModalView = ExitFromBuildModeView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(exitFromBiWModalView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            exitFromBiWModalView.gameObject.SetActive(!isActive);

            // Act
            exitFromBiWModalView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, exitFromBiWModalView.gameObject.activeSelf, "Game object activate property does not match!");
        }

        [Test]
        public void CancelExitCorrectly()
        {
            // Arrange
            bool canceled = false;
            exitFromBiWModalView.OnCancelExit += () => { canceled = true; };

            // Act
            exitFromBiWModalView.CancelExit();

            // Assert
            Assert.IsTrue(canceled, "The canceled flag is false!");
        }

        [Test]
        public void ConfirmExitCorrectly()
        {
            // Arrange
            bool confirmed = false;
            exitFromBiWModalView.OnConfirmExit += () => { confirmed = true; };

            // Act
            exitFromBiWModalView.ConfirmExit();

            // Assert
            Assert.IsTrue(confirmed, "The confirmed flag is false!");
        }
    }
}