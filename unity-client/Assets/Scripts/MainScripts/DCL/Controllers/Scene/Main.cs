using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    public class Main : MonoBehaviour
    {
        public static Main i { get; private set; }

        [NonSerialized]
        public SceneController sceneController;

        [FormerlySerializedAs("factoryManifest")]
        public DCLComponentFactory componentFactory;

        public bool startDecentralandAutomatically = true;

        public DebugConfig debugConfig;

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

            DataStore.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.debugConfig.msgStepByStep = debugConfig.msgStepByStep;

            sceneController = new SceneController();
            sceneController.Initialize(componentFactory);
        }

        private void Start()
        {
            sceneController.Start();
        }

        private void Update()
        {
            sceneController.Update();
        }

        private void LateUpdate()
        {
            sceneController.LateUpdate();
        }

        private void OnDestroy()
        {
            sceneController.OnDestroy();
        }

        public bool ProcessMessage(MessagingBus.QueuedSceneMessage_Scene msgObject, out CleanableYieldInstruction yieldInstruction)
        {
            return sceneController.ProcessMessage(msgObject, out yieldInstruction);
        }

        public void LoadParcelScenesExecute(string decentralandSceneJSON)
        {
            sceneController.LoadParcelScenesExecute(decentralandSceneJSON);
        }

        public void UnloadParcelSceneExecute(string sceneKey)
        {
            sceneController.UnloadParcelSceneExecute(sceneKey);
        }

        public void UnloadAllScenes()
        {
            sceneController.UnloadAllScenes();
        }

        public void UpdateParcelScenesExecute(string sceneKey)
        {
            sceneController.UpdateParcelScenesExecute(sceneKey);
        }

        public void EnqueueSceneMessage(MessagingBus.QueuedSceneMessage_Scene message)
        {
            sceneController.EnqueueSceneMessage(message);
        }
    }
}