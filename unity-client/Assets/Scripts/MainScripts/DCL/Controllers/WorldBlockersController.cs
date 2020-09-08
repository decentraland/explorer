﻿using UnityEngine;

namespace DCL.Controllers
{
    public class WorldBlockersController : MonoBehaviour
    {
        WorldBlockerHandler blockerHandler;

        void Start()
        {
            blockerHandler = new WorldBlockerHandler();

            SceneController.i.OnNewSceneAdded += OnNewSceneAdded;
            DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnWorldReposition;

            SceneController.OnDebugModeSet += () =>
            {
                blockerHandler.CleanBlockers();
            };
        }

        void OnDisable()
        {
            SceneController.i.OnNewSceneAdded -= OnNewSceneAdded;
            DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnWorldReposition;
        }

        void SetupWorldBlockers()
        {
            blockerHandler.SetupGlobalBlockers(SceneController.i.loadedScenes, 100, transform);
        }

        void OnWorldReposition(DCLCharacterPosition charPos)
        {
            var newPosition = charPos.WorldToUnityPosition(Vector3.zero);
            transform.position = newPosition;
        }

        void OnNewSceneAdded(ParcelScene newScene)
        {
            if (SceneController.i.isDebugMode) return;

            newScene.OnSceneReady += OnSceneReady;
        }

        void OnSceneReady(ParcelScene scene)
        {
            scene.OnSceneReady -= OnSceneReady;

            if (SceneController.i.isDebugMode)
            {
                blockerHandler.CleanBlockers();
                return;
            }

            SetupWorldBlockers();
        }
    }
}