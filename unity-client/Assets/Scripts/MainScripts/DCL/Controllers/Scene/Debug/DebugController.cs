using System;
using UnityEngine.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class DebugController : IDisposable
    {
        public DebugConfig debugConfig => DataStore.debugConfig;

        public DebugView debugView;

        public event Action OnDebugModeSet;

        public DebugController()
        {
            GameObject view = Object.Instantiate(UnityEngine.Resources.Load("DebugView")) as GameObject;
            debugView = view.GetComponent<DebugView>();
        }

        public void SetDebug()
        {
            Debug.unityLogger.logEnabled = true;

            debugConfig.isDebugMode = true;
            debugView.ShowFPSPanel();

            SceneController.i.InitializeSceneBoundariesChecker(true);

            OnDebugModeSet?.Invoke();

            //NOTE(Brian): Added this here to prevent the SetDebug() before Awake()
            //             case. Calling Initialize multiple times in a row is safe.
            Environment.i.Initialize(SceneController.i);
            Environment.i.worldBlockersController.SetEnabled(false);
        }

        public void HideFPSPanel()
        {
            debugView.HideFPSPanel();
        }

        public void ShowFPSPanel()
        {
            debugView.ShowFPSPanel();
        }

        public void SetSceneDebugPanel()
        {
            debugView.SetSceneDebugPanel();
        }

        public void SetEngineDebugPanel()
        {
            debugView.SetEngineDebugPanel();
        }

        public void Dispose()
        {
            Object.Destroy(debugView.gameObject);
        }
    }
}