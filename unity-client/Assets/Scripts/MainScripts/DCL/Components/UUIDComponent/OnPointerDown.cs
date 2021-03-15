using DCL.Interface;
using DCL.Helpers;
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class OnPointerDown : OnPointerEvent
    {
        public const string NAME = "pointerDown";

        public virtual void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled || !IsVisible()) return;

            Model model = this.model as OnPointerEvent.Model;

            if (ShouldReportEvent(buttonId, hit))
            {
                string meshName = GetMeshName(hit.collider);

                DCL.Interface.WebInterface.ReportOnPointerDownEvent(buttonId, scene.sceneData.id, model.uuid, entity.entityId, meshName, ray, hit.point, hit.normal, hit.distance);
            }
        }

        protected bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            Model model = this.model as OnPointerEvent.Model;
            return IsVisible() && IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button);
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_DOWN;
        }
    }
}