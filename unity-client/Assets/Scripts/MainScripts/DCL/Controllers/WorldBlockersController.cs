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

        void OnNewSceneAdded(ParcelScene newScene)
        {
            if (!subscribedToWorldReposition && DCLCharacterController.i)
            {
                DCLCharacterController.i.characterPosition.OnPrecisionAdjust += OnWorldReposition;
                subscribedToWorldReposition = true;
            }

            blockerHandler.SetupGlobalBlockers(SceneController.i.loadedScenes, 100, transform);
        }

        void OnWorldReposition(DCLCharacterPosition charPos)
        {
            var newPosition = charPos.WorldToUnityPosition(Vector3.zero);
            transform.position = newPosition;
        }
    }
}