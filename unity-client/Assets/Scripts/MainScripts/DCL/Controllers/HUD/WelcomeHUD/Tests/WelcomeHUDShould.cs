using NUnit.Framework;
using System.Collections;
using UnityEngine;

namespace Tests
{
    public class WelcomeHUDShould : TestsBase
    {
        WelcomeHUDController controller;
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new WelcomeHUDController();
            var model = new WelcomeHUDController.Model()
            {
                hasWallet = true
            };

            controller.Initialize(model);

        }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.IsTrue(controller.view != null);
        }


        [Test]
        public void BehaveCorrectlyAfterCloseButtonIsPressed()
        {
            Assert.IsTrue(Cursor.lockState == CursorLockMode.None);

            controller.view.closeButton.onClick.Invoke();

            Assert.IsTrue(Cursor.lockState == CursorLockMode.Locked);
            Assert.IsFalse(controller.view.gameObject.activeSelf);
        }

        [Test]
        public void BehaveCorrectlyAfterConfirmButtonIsPressed()
        {
            Assert.IsTrue(Cursor.lockState == CursorLockMode.None);

            controller.view.confirmButton.onClick.Invoke();

            Assert.IsTrue(Cursor.lockState == CursorLockMode.Locked);
            Assert.IsFalse(controller.view.gameObject.activeSelf);
        }

    }
}
