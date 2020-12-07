using System;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("debugReferences")] public DebugView debugView;

        // Beware this SetDebug() may be called before Awake() somehow...
        [ContextMenu("Set Debug mode")]
        public void SetDebug()
        {
            Debug.unityLogger.logEnabled = true;

            DebugConfig debugConfig = Environment.i.debugConfig;

            debugConfig.isDebugMode = true;
            debugView.fpsPanel.SetActive(true);

            SceneController.i.InitializeSceneBoundariesChecker(true);

            OnDebugModeSet?.Invoke();

            //NOTE(Brian): Added this here to prevent the SetDebug() before Awake()
            //             case. Calling Initialize multiple times in a row is safe.
            Environment.i.Initialize(SceneController.i, SceneController.i);
            Environment.i.worldBlockersController.SetEnabled(false);
        }

        public void HideFPSPanel()
        {
            debugView.fpsPanel.SetActive(false);
        }

        public void ShowFPSPanel()
        {
            debugView.fpsPanel.SetActive(true);
        }

        public void SetSceneDebugPanel()
        {
            debugView.engineDebugPanel.SetActive(false);
            debugView.sceneDebugPanel.SetActive(true);
        }

        public void SetEngineDebugPanel()
        {
            debugView.sceneDebugPanel.SetActive(false);
            debugView.engineDebugPanel.SetActive(true);
        }
    }
}