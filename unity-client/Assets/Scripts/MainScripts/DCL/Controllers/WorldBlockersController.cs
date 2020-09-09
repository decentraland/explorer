using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public class WorldBlockersController
    {
        SceneController sceneController;
        Transform blockersParent;
        BlockerHandler blockerHandler;
        HashSet<Vector2Int> allLoadedParcelCoords = new HashSet<Vector2Int>();

        public WorldBlockersController(SceneController sceneController, Transform blockersParent)
        {
            blockerHandler = new BlockerHandler();

            this.sceneController = sceneController;

            blockersParent.position = Vector3.zero;
            this.blockersParent = blockersParent;

            sceneController.OnNewSceneAdded += OnNewSceneAdded;
            DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnWorldReposition;

            SceneController.OnDebugModeSet += () =>
            {
                blockerHandler.CleanBlockers();
            };
        }

        void OnDisable()
        {
            sceneController.OnNewSceneAdded -= OnNewSceneAdded;
            DCLCharacterController.i.characterPosition.OnPrecisionAdjust -= OnWorldReposition;
        }

        void SetupWorldBlockers()
        {
            allLoadedParcelCoords.Clear();

            // Create fast (hashset) collection of loaded parcels coords
            foreach (var element in sceneController.loadedScenes)
            {
                if (!element.Value.isReady) continue;

                allLoadedParcelCoords.UnionWith(element.Value.parcels);
            }

            blockerHandler.SetupGlobalBlockers(allLoadedParcelCoords, 100, blockersParent);
        }

        void OnWorldReposition(DCLCharacterPosition charPos)
        {
            var newPosition = charPos.WorldToUnityPosition(Vector3.zero); // Blockers parent original position
            blockersParent.position = newPosition;
        }

        void OnNewSceneAdded(ParcelScene newScene)
        {
            if (sceneController.isDebugMode) return;

            newScene.OnSceneReady += OnSceneReady;
        }

        void OnSceneReady(ParcelScene scene)
        {
            scene.OnSceneReady -= OnSceneReady;

            if (sceneController.isDebugMode)
            {
                blockerHandler.CleanBlockers();
                return;
            }

            SetupWorldBlockers();
        }
    }
}