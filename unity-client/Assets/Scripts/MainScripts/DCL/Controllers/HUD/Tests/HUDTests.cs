using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class HUDControllerShould : TestsBase
    {
        [UnityTest]
        public IEnumerator NotCreateHUDsInitially()
        {
            yield return InitScene();
            // There must be a hud controller
            HUDController hudController = HUDController.i;
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            // But the HUDs should not have been created
            Assert.IsNull(hudController.avatarHud);
            Assert.IsNull(hudController.notificationHud);
            Assert.IsNull(hudController.avatarEditorHud);
            Assert.IsNull(hudController.minimapHud);

            // And the HUD views should not exist
            Assert.IsNull(GameObject.FindObjectOfType<AvatarHUDView>());
            Assert.IsNull(GameObject.FindObjectOfType<NotificationHUDView>());
            Assert.IsNull(GameObject.FindObjectOfType<AvatarEditorHUDView>());
            Assert.IsNull(GameObject.FindObjectOfType<MinimapHUDView>());
        }

        [UnityTest]
        public IEnumerator CreateHudIfConfigurationIsActive()
        {
            yield return InitScene();
            // There must be a hud controller
            HUDController hudController = HUDController.i;
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            HUDConfiguration config = new HUDConfiguration() { active = true, visible = true };
            hudController.ConfigureHUDElement(HUDController.HUDElementID.AVATAR, JsonUtility.ToJson(config));
            hudController.ConfigureHUDElement(HUDController.HUDElementID.NOTIFICATION, JsonUtility.ToJson(config));

            yield return null;

            // HUD controllers are created
            Assert.IsNotNull(hudController.avatarHud);
            Assert.IsNotNull(hudController.notificationHud);

            // HUD views exist
            Assert.IsNotNull(GameObject.FindObjectOfType<AvatarHUDView>());
            Assert.IsNotNull(GameObject.FindObjectOfType<NotificationHUDView>());
        }
    }
}
