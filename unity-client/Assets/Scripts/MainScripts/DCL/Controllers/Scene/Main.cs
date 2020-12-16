﻿using System;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    /// <summary>
    /// This is the InitialScene entry point.
    /// Most of the application subsystems should be initialized from this class Awake() event.
    /// </summary>
    public class Main : MonoBehaviour
    {
        public static Main i { get; private set; }

        public DCLComponentFactory componentFactory;

        public DebugConfig debugConfig;

        private PerformanceMetricsController performanceMetricsController;

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

#if !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);
            Debug.unityLogger.logEnabled = false;
#endif

            DataStore.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.debugConfig.msgStepByStep = debugConfig.msgStepByStep;

            performanceMetricsController = new PerformanceMetricsController();

            RenderProfileManifest.i.Initialize();
            Environment.SetupWithDefaults();
        }

        private void Start()
        {
            Environment.i.world.sceneController.Start();
        }

        private void Update()
        {
            Environment.i.world.sceneController.Update();
            performanceMetricsController?.Update();
        }

        private void LateUpdate()
        {
            Environment.i.world.sceneController.LateUpdate();
        }

        private void OnDestroy()
        {
            Environment.i.world.sceneController.Dispose();
        }

        #region RuntimeMessagingBridge

        public void LoadParcelScenes(string payload)
        {
            Environment.i.world.sceneController.LoadParcelScenes(payload);
        }

        public void SendSceneMessage(string payload)
        {
            Environment.i.world.sceneController.SendSceneMessage(payload);
        }

        public void UnloadScene(string sceneId)
        {
            Environment.i.world.sceneController.UnloadScene(sceneId);
        }

        public void CreateUIScene(string payload)
        {
            Environment.i.world.sceneController.CreateUIScene(payload);
        }

        public void UpdateParcelScenes(string payload)
        {
            Environment.i.world.sceneController.UpdateParcelScenes(payload);
        }

        #endregion

        public void BuilderReady()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}