using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestsBase
{
    private static bool sceneInitialized = false;
    protected SceneController sceneController;
    protected ParcelScene scene;
    protected CameraController cameraController;

    [UnitySetUp]
    protected virtual IEnumerator SetUp()
    {
        if (!sceneInitialized)
        {
            yield return InitUnityScene("MainTest");
            sceneInitialized = true;
        }

        SetUp_Camera();
        yield return SetUp_SceneController();
        yield return SetUp_CharacterController();
    }


    [UnityTearDown]
    protected virtual IEnumerator TearDown()
    {
        yield return null;

        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();

        MemoryManager.i?.CleanupPoolsIfNeeded(true);
        PoolManager.i?.Cleanup();
        PointerEventsController.i?.Cleanup();
        MessagingControllersManager.i?.Cleanup();

        Caching.ClearCache();
        Resources.UnloadUnusedAssets();

        yield return null;
    }

    protected IEnumerator InitUnityScene(string sceneName = null)
    {
        yield return TestHelpers.UnloadAllUnityScenes();

        Scene? newScene;

        if (string.IsNullOrEmpty(sceneName))
        {
            newScene = SceneManager.CreateScene(TestHelpers.testingSceneName + (TestHelpers.testSceneIteration++));
            if (newScene.HasValue)
            {
                SceneManager.SetActiveScene(newScene.Value);
            }
        }
        else
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }
    }

    public void SetUp_TestScene()
    {
        scene = sceneController.CreateTestScene();
    }

    public virtual IEnumerator SetUp_CharacterController()
    {
        if (DCLCharacterController.i == null)
        {
            GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
        }

        yield return null;
        Assert.IsTrue(DCLCharacterController.i != null);
    }

    public virtual void SetUp_Camera()
    {
        cameraController = GameObject.FindObjectOfType<CameraController>();

        if (cameraController == null)
            cameraController = GameObject.Instantiate(Resources.Load<GameObject>("CameraController")).GetComponent<CameraController>();
    }

    public virtual IEnumerator SetUp_SceneController(bool debugMode = false, bool usesWebServer = false, bool spawnTestScene = true)
    {
        PoolManager.enablePrewarm = false;
        sceneController = TestHelpers.InitializeSceneController(usesWebServer);
        sceneController.deferredMessagesDecoding = false;
        sceneController.prewarmSceneMessagesPool = false;

        if (debugMode)
            sceneController.SetDebug();

        yield return new WaitForSeconds(0.01f);

        if (spawnTestScene)
            SetUp_TestScene();
    }

    private void SetUp_UIScene()
    {
        string globalSceneId = "global-scene";

        sceneController.CreateUIScene(
            JsonConvert.SerializeObject(
                new CreateUISceneMessage
                {
                    id = globalSceneId,
                    baseUrl = "",
                })
        );
    }


    protected virtual IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true, bool spawnTestScene = true, bool spawnUIScene = true, bool debugMode = false, bool reloadUnityScene = true)
    {
        yield return InitUnityScene("MainTest");

        yield return SetUp_SceneController(debugMode, usesWebServer, spawnTestScene);

        if (spawnCharController)
        {
            yield return SetUp_CharacterController();
        }

        var newPos = new Vector3(10, 0, 10);
        DCLCharacterController.i.SetPosition(newPos);
        yield return null;

        if (spawnUIScene)
        {
            SetUp_UIScene();
        }

        PointerEventsController.i.Initialize(isTesting: true);
    }


    protected IEnumerator WaitForUICanvasUpdate()
    {
        yield break;
    }

    public static T Reflection_GetStaticField<T>(Type baseType, string fieldName)
    {
        return (T)baseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }

    public static T Reflection_GetField<T>(object instance, string fieldName)
    {
        return (T)instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }

}
