using DCL.Interface;
using DCL.Helpers;
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class OnPointerUp : OnPointerEvent
    {
        public const string NAME = "pointerUp";

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit, bool isHitInfoValid)
        {
            if (!enabled || !IsVisible()) return;

            if (IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button))
            {
                string meshName = null;

                if (isHitInfoValid)
                    meshName = GetMeshName(hit.collider);

                DCL.Interface.WebInterface.ReportOnPointerUpEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance, isHitInfoValid);
            }
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_UP;
        }
    }
}