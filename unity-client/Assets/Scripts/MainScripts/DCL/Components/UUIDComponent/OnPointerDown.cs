using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class OnPointerDown : OnPointerEvent
    {
        public const string NAME = "pointerDown";
        public event System.Action OnPointerDownReport; // TODO: move this to a new child class to be used by the AvatarShape

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled) return;

            if (IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button))
            {
                string meshName = GetMeshName(hit.collider);

                DCL.Interface.WebInterface.ReportOnPointerDownEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance);

                OnPointerDownReport?.Invoke();
            }
        }
    }
}