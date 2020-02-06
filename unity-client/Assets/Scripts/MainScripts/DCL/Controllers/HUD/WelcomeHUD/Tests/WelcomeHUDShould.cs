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
        }

        [Test]
        public void BeCreatedProperly()
        {
            Assert.IsTrue(controller.view != null);
        }


        [Test]
        public void InitializeProperly()
        {
            var model = new WelcomeHUDController.Model()
            {
                title = "test title",

                timeTarget = 0,
                timeText = "test time",
                bodyText = "test body",
                buttonText = "test buttonText",
                buttonAction = "test buttonAction",

                showTime = true,
                showButton = true
            };

            WelcomeHUDController.ENABLE_DYNAMIC_CONTENT = true;
            controller.Initialize(model);

            Assert.IsTrue(controller.model == model);
            Assert.IsTrue(controller.view.bodyText.text == model.bodyText);
            Assert.IsTrue(controller.view.headerText1.text == model.title);
            Assert.IsTrue(controller.view.headerText2.text == model.timeText);
            Assert.IsTrue(controller.view.buttonText.text == model.buttonText);

            Assert.IsTrue(controller.view.headerText2.gameObject.activeSelf);
            Assert.IsTrue(controller.view.confirmButton.gameObject.activeSelf);

            model.showTime = false;
            model.showButton = false;
            controller.Initialize(model);

            Assert.IsFalse(controller.view.headerText2.gameObject.activeSelf);
            Assert.IsFalse(controller.view.confirmButton.gameObject.activeSelf);
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
