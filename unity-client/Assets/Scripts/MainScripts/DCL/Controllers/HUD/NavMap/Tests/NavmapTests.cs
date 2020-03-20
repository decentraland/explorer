using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Tests
{
    public class NavmapTests : TestsBase
    {
        DCL.NavmapView navmapView;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            navmapView = GameObject.FindObjectOfType<DCL.NavmapView>();
        }

        [UnityTest]
        public IEnumerator Toggle()
        {
            InputAction_Trigger action = null;
            var inputController = GameObject.FindObjectOfType<InputController>();
            for (int i = 0; i < inputController.triggerTimeActions.Length; i++)
            {
                // Find the open nav map action used by the input controller
                if (inputController.triggerTimeActions[i].GetDCLAction() == DCLAction_Trigger.ToggleNavMap)
                {
                    action = inputController.triggerTimeActions[i];
                    break;
                }
            }

            Assert.IsNotNull(action);

            action.RaiseOnTriggered();

            yield return null;

            Assert.IsTrue(Reflection_GetField<ScrollRect>(navmapView, "scrollRect").gameObject.activeSelf);

            action.RaiseOnTriggered();

            yield return null;

            Assert.IsFalse(Reflection_GetField<ScrollRect>(navmapView, "scrollRect").gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator SetSceneName()
        {
            const string sceneName = "SCENE_NAME";

            var controller = new MinimapHUDController();
            controller.UpdateSceneName(sceneName);

            yield return null;

            Assert.AreEqual(sceneName, Reflection_GetField<TextMeshProUGUI>(navmapView, "currentSceneNameText").text);
        }

        [UnityTest]
        public IEnumerator SetPlayerCoordinatesVector2()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);

            var controller = new MinimapHUDController();
            controller.UpdatePlayerPosition(coords);

            yield return null;

            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(navmapView, "currentSceneCoordsText").text);
        }

        [UnityTest]
        public IEnumerator SetPlayerCoordinatesString()
        {
            Vector2 coords = new Vector2(Random.Range(-150, 150), Random.Range(-150, 150));
            string coordString = string.Format("{0},{1}", coords.x, coords.y);

            var controller = new MinimapHUDController();
            controller.UpdatePlayerPosition(coordString);

            yield return null;

            Assert.AreEqual(coordString, Reflection_GetField<TextMeshProUGUI>(navmapView, "currentSceneCoordsText").text);
        }
    }
}