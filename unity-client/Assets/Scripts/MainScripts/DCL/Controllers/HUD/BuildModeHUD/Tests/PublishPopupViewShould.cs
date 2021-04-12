using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class PublishPopupViewShould
    {
        private PublishPopupView publishPopupView;

        [SetUp]
        public void SetUp() { publishPopupView = PublishPopupView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(publishPopupView.gameObject); }

        [Test]
        public void PublishStartCorrectly()
        {
            // Arrange
            publishPopupView.gameObject.SetActive(false);
            publishPopupView.loadingBar.SetActive(false);
            publishPopupView.resultText.gameObject.SetActive(true);

            // Act
            publishPopupView.PublishStart();

            // Assert
            Assert.IsTrue(publishPopupView.gameObject.activeSelf, "game object activate property is false!");
            Assert.IsTrue(publishPopupView.loadingBar.activeSelf, "loadingBar activate property is false!");
            Assert.IsFalse(publishPopupView.resultText.gameObject.activeInHierarchy, "resultText activate property is true!");
        }

        [Test]
        public void PublishEndCorrectly()
        {
            // Arrange
            string message = "Test message";
            publishPopupView.loadingBar.SetActive(true);
            publishPopupView.resultText.gameObject.SetActive(false);
            publishPopupView.resultText.text = "";
            publishPopupView.closeButton.gameObject.SetActive(false);

            // Act
            publishPopupView.PublishEnd(message);

            // Assert
            Assert.IsFalse(publishPopupView.loadingBar.activeSelf, "loadingBar activate property is false!");
            Assert.IsTrue(publishPopupView.resultText.gameObject.activeInHierarchy, "resultText activate property is false!");
            Assert.AreEqual(message, publishPopupView.resultText.text, "Publish Status text does not match!");
            Assert.IsTrue(publishPopupView.closeButton.gameObject.activeInHierarchy, "closeButton activate property is false!");
        }
    }
}