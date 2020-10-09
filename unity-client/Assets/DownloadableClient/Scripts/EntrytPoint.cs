using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DCL;
using DCL.Components;
//using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
//using DCL.Interface;
using DCL.Models;

namespace DownloadableClient
{
    public class EntrytPoint : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI logText;
        [SerializeField] Canvas canvas;
        [SerializeField] Camera sceneCamera;

        private Scene thisScene;

        private void Awake()
        {
            thisScene = SceneManager.GetActiveScene();
            Application.logMessageReceived += OnLogMessageReceived;
            logText.text = "";
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(LoadInitialScene());
        }

        private IEnumerator LoadInitialScene()
        {
            AsyncOperation sceneOperation = SceneManager.LoadSceneAsync("InitialScene", LoadSceneMode.Additive);
            yield return sceneOperation;

            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;

            DCL.WSSController.i.openBrowserWhenStart = true;
            //DCL.WSSController.i.baseUrlMode = DCL.WSSController.BaseUrl.LOCAL_HOST;
            //DCL.WSSController.i.forceLocalComms = true;
            DCL.WSSController.i.baseUrlMode = DCL.WSSController.BaseUrl.CUSTOM;
            DCL.WSSController.i.baseUrlCustom = "https://explorer.decentraland.org/?";
            DCL.WSSController.i.forceLocalComms = false;
            DCL.WSSController.i.environment = DCL.WSSController.Environment.ORG;
            DCL.WSSController.i.debugPanelMode = DCL.WSSController.DebugPanel.Off;

            sceneCamera.GetComponent<AudioListener>().enabled = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != thisScene)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.SetActiveScene(scene);
            }
        }

        private void OnRenderingStateChanged(bool newValue, bool oldValue)
        {
            if (newValue)
            {
                CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
                Application.logMessageReceived -= OnLogMessageReceived;
                canvas.gameObject.SetActive(false);
                sceneCamera.gameObject.SetActive(false);
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            logText.text += string.Format("\n{0}", condition);
        }
    }
}