using UnityEngine;
using System.Collections;

public class TutorialStageController : MonoBehaviour
{
    public virtual void OnStageStart() { }
    public virtual void OnStageFinished() { }

    public virtual IEnumerator ShowTooltip(TutorialTooltip tooltip)
    {
        if (tooltip != null)
        {
            tooltip.Show();
            yield return WaitForSecondsCache.Get(TutorialController.TOOLTIP_AUTO_HIDE_SECONDS);
            tooltip.Hide();
        }
    }

    public virtual IEnumerator WaitIdleTime()
    {
        yield return WaitForSecondsCache.Get(TutorialController.DEFAULT_STAGE_IDLE_TIME);
    }
}
