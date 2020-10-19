using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;
using DCL.Tutorial;
using NSubstitute;

public class TestsBase
{
    private const bool DEBUG_PAUSE_ON_INTEGRITY_FAIL = false;

    protected Component[] startingSceneComponents = null;
    protected bool sceneInitialized = false;
    protected SceneController sceneController;
    protected ParcelScene scene;
    protected CameraController cameraController;

    /// <summary>
    /// Use this as a parent for your dynamically created gameobjects in tests
    /// so they are cleaned up automatically in the teardown
    /// </summary>
    private GameObject runtimeGameObjectsRoot;

    protected virtual bool justSceneSetUp => false;
    protected virtual bool enableSceneIntegrityChecker => true;

    [UnitySetUp]
    protected virtual IEnumerator SetUp()
    {
        DCL.Configuration.EnvironmentSettings.RUNNING_TESTS = true;

        if (!sceneInitialized)
        {
            yield return InitUnityScene("MainTest");
            sceneInitialized = true;
        }

        if (justSceneSetUp)
        {
            yield return SetUp_SceneIntegrityChecker();
            SetUp_Renderer();
            Environment.i.Initialize(new DummyMessageHandler(), Substitute.For<ISceneHandler>());
            yield break;
        }

        SetUp_Camera();

        yield return SetUp_SceneController();
        yield return SetUp_CharacterController();

        yield return SetUp_SceneIntegrityChecker();

        SetUp_Renderer();
        runtimeGameObjectsRoot = new GameObject("_RuntimeGameObjectsRoot");
        Environment.i.Initialize(new DummyMessageHandler(), Substitute.For<ISceneHandler>());
    }


    [UnityTearDown]
    protected virtual IEnumerator TearDown()
    {
        yield return null;

        if (runtimeGameObjectsRoot != null)
            Object.Destroy(runtimeGameObjectsRoot.gameObject);

        TestHelpers.ForceUnloadAllScenes(SceneController.i);

        Environment.i.Cleanup();

        if (DCLCharacterController.i != null)
        {
            DCLCharacterController.i.ResumeGravity();
            DCLCharacterController.i.enabled = true;

            if (DCLCharacterController.i.characterController != null)
                DCLCharacterController.i.characterController.enabled = true;
        }

        yield return TearDown_Memory();

        if (MapRenderer.i != null)
            MapRenderer.i.Cleanup();

        yield return TearDown_SceneIntegrityChecker();
    }

    protected void TearDown_PromiseKeepers()
    {
        AssetPromiseKeeper_GLTF.i?.Cleanup();
        AssetPromiseKeeper_AB_GameObject.i?.Cleanup();
        AssetPromiseKeeper_AB.i?.Cleanup();
    }

    protected IEnumerator TearDown_Memory()
    {
        TearDown_PromiseKeepers();

        if (Environment.i.memoryManager != null)
            yield return Environment.i.memoryManager.CleanupPoolsIfNeeded(true);

        if (PoolManager.i != null)
            PoolManager.i.Cleanup();

        Caching.ClearCache();
        Resources.UnloadUnusedAssets();
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
        DCLCharacterController.i.gameObject.SetActive(true);
        DCLCharacterController.i.characterController.enabled = true;
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

        if (debugMode)
            sceneController.SetDebug();

        yield return null;

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

    public virtual void SetUp_Renderer()
    {
        CommonScriptableObjects.rendererState.Set(true);
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
    }


    protected IEnumerator WaitForUICanvasUpdate()
    {
        yield break;
    }

    public static T Reflection_GetStaticField<T>(System.Type baseType, string fieldName)
    {
        return (T) baseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }

    public static T Reflection_GetField<T>(object instance, string fieldName)
    {
        return (T) instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }

    public static void Reflection_SetField<T>(object instance, string fieldName, T newValue)
    {
        instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, newValue);
    }

    protected IEnumerator SetUp_SceneIntegrityChecker()
    {
        if (!enableSceneIntegrityChecker)
            yield break;

        //NOTE(Brian): to make it run faster in CI
        if (Application.isBatchMode)
            yield break;

        yield return null;
        startingSceneComponents = Object.FindObjectsOfType<Component>();
    }

    protected IEnumerator TearDown_SceneIntegrityChecker()
    {
        if (!enableSceneIntegrityChecker)
            yield break;

        //NOTE(Brian): to make it run faster in CI
        if (Application.isBatchMode)
            yield break;

        if (startingSceneComponents == null)
        {
            Debug.LogError("SceneIntegrityChecker fail. TearDown called without SetUp or SetUp_SceneIntegrityChecker?");
            yield break;
        }

        //NOTE(Brian): If any Destroy() calls are pending, this will flush them.
        yield return null;

        Component[] objects = Object.FindObjectsOfType<Component>();

        List<Component> newObjects = new List<Component>();

        foreach (var o in objects)
        {
            if (o.ToString().Contains("MainCamera"))
                continue;

            if (!startingSceneComponents.Contains(o))
            {
                newObjects.Add(o);
            }
        }

        if (newObjects.Count > 0)
        {
            Debug.LogError("Dangling components detected!. Look your TearDown code, you missed to destroy objects after the tests?.");

            //NOTE(Brian): Can't use asserts here because Unity Editor hangs for some reason.
            foreach (var o in newObjects)
            {
                if (DEBUG_PAUSE_ON_INTEGRITY_FAIL && !Application.isBatchMode)
                    Debug.LogError($"Component - {o} (Click to highlight)", o.gameObject);
                else
                    Debug.LogError($"Component - {o}", o.gameObject);
            }

            if (DEBUG_PAUSE_ON_INTEGRITY_FAIL && !Application.isBatchMode)
            {
                Debug.Break();
                yield return null;
            }
        }
    }

    protected GameObject CreateTestGameObject(string name)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(runtimeGameObjectsRoot.transform);
        return gameObject;
    }
}