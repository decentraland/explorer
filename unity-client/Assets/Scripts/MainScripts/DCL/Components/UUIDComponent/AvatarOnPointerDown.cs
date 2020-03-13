using DCL.Interface;
using UnityEngine;
using DCL.Helpers;

namespace DCL.Components
{
    public class AvatarOnPointerDown : OnPointerDown
    {
        public event System.Action OnPointerDownReport;

        public override void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled) return;

            if (ShouldReportEvent(buttonId, hit))
            {
                base.Report(buttonId, ray, hit);

                OnPointerDownReport?.Invoke();
            }
        }
    }
}