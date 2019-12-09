using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace Tests
{
    public class UITestsBase : TestsBase
    {
        protected override IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
        {
            yield return base.InitScene(usesWebServer, spawnCharController, spawnTestScene, spawnUIScene, debugMode, reloadUnityScene);

            if (spawnCharController)
            {
                DCLCharacterController.i.gravity = 0f;
                TestHelpers.SetCharacterPosition(new Vector3(8f, 0f, 8f));
            }
        }
    }
}
