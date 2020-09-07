using UnityEngine;

namespace DCL.Controllers
{
    public class WorldBlockersController : MonoBehaviour
    {
        BlockerHandler blockerHandler;
        bool subscribedToWorldReposition = false;

        void Start()
        {
            blockerHandler = new BlockerHandler();

            SceneController.i.OnNewSceneAdded += OnNewSceneAdded;
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
            newScene.OnSceneReady += OnSceneReady;
            if (!subscribedToWorldReposition && DCLCharacterController.i)
            {
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnWorldReposition;
                subscribedToWorldReposition = true;
            }

            SetupWorldBlockers();
        }

        void OnSceneReady(ParcelScene scene)
        {
            scene.OnSceneReady -= OnSceneReady;

            SetupWorldBlockers();
        }
    }
}