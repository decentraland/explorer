using UnityEngine;
using DCL.Interface;
using DCL.Helpers;
using DCL.Models;

namespace DCL.Components
{
    public class OnClick : OnPointerEvent
    {
        public const string NAME = "onClick";

        public void Report(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            if (!enabled || !IsVisible()) return;

            OnPointerEvent.Model model = this.model as OnPointerEvent.Model;

            if (IsAtHoverDistance(hit.distance) && (model.button == "ANY" || buttonId.ToString() == model.button))
                DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.UUID_ON_CLICK;
        }
    }
}