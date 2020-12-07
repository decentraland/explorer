using System;
using UnityEngine;

namespace DCL
{
    public class DebugBridge : MonoBehaviour
    {
        public static DebugBridge i { get; private set; }

        public void Awake()
        {
            i = this;
        }

        public static Action OnDebugModeSet;

        public DebugReferences debugReferences;

        // Beware this SetDebug() may be called before Awake() somehow...
        [ContextMenu("Set Debug mode")]
        public void SetDebug()
        {
            Debug.unityLogger.logEnabled = true;

            DebugConfig debugConfig = Environment.i.debugConfig;

            debugConfig.isDebugMode = true;
            debugReferences.fpsPanel.SetActive(true);

            SceneController.i.InitializeSceneBoundariesChecker(true);

            OnDebugModeSet?.Invoke();

            //NOTE(Brian): Added this here to prevent the SetDebug() before Awake()
            //             case. Calling Initialize multiple times in a row is safe.
            Environment.i.Initialize(SceneController.i, SceneController.i);
            Environment.i.worldBlockersController.SetEnabled(false);
        }

        public void HideFPSPanel()
        {
            debugReferences.fpsPanel.SetActive(false);
        }

        public void ShowFPSPanel()
        {
            debugReferences.fpsPanel.SetActive(true);
        }

        public void SetSceneDebugPanel()
        {
            debugReferences.engineDebugPanel.SetActive(false);
            debugReferences.sceneDebugPanel.SetActive(true);
        }

        public void SetEngineDebugPanel()
        {
            debugReferences.sceneDebugPanel.SetActive(false);
            debugReferences.engineDebugPanel.SetActive(true);
        }
    }
}