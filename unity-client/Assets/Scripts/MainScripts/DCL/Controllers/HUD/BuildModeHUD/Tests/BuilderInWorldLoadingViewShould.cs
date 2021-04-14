using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace Tests.BuildModeHUDViews
{
    public class BuilderInWorldLoadingViewShould
    {
        private BuilderInWorldLoadingView builderInWorldLoadingView;

        [SetUp]
        public void SetUp() { builderInWorldLoadingView = BuilderInWorldLoadingView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(builderInWorldLoadingView.gameObject); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShowCorrectly(bool showTips)
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(false);
            builderInWorldLoadingView.loadingTipItem.tipText.text = string.Empty;

            // Act
            builderInWorldLoadingView.Show(showTips);

            // Assert
            Assert.IsTrue(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is false!");
            if (showTips && builderInWorldLoadingView.loadingTips.Count > 0)
            {
                Assert.IsNotEmpty(builderInWorldLoadingView.loadingTipItem.tipText.text, "tipsText is empty!");
                Assert.IsTrue(builderInWorldLoadingView.loadingTips.Any(x => x.tipMessage == builderInWorldLoadingView.loadingTipItem.tipText.text), "The set tipsText does not match!");
            }
            else
                Assert.IsEmpty(builderInWorldLoadingView.loadingTipItem.tipText.text, "tipsText is not empty!");
        }

        [Test]
        public void HideCorrectly()
        {
            // Arrange
            builderInWorldLoadingView.gameObject.SetActive(true);

            // Act
            builderInWorldLoadingView.Hide(true);

            // Assert
            Assert.IsFalse(builderInWorldLoadingView.gameObject.activeSelf, "The view activeSelf property is true!");
        }

        [Test]
        public void CancelLoadingCorrectly()
        {
            // Arrange
            bool loadingCanceled = false;
            builderInWorldLoadingView.OnCancelLoading += () => { loadingCanceled = true; };

            // Act
            builderInWorldLoadingView.CancelLoading(new DCLAction_Trigger());

            // Assert
            Assert.IsTrue(loadingCanceled, "loadingCanceled is false!");
        }
    }
}