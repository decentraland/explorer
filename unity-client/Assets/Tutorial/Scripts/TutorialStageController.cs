using UnityEngine;
using System.Collections;

public class TutorialStageController : MonoBehaviour
{
    private bool rendererEnabled = true;

    public virtual void OnStageStart()
    {
        if (RenderingController.i)
        {
            RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
        }
    }
    public virtual void OnStageFinished()
    {
        if (RenderingController.i)
        {
            RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
        }
    }

    public virtual IEnumerator ShowTooltip(TutorialTooltip tooltip, bool autoHide = true)
    {
        if (tooltip != null)
        {
            tooltip.Show();
            if (autoHide)
            {
                yield return WaitSeconds(tooltip.secondsOnScreen);
                tooltip.Hide();
            }
        }
    }

    public virtual void HideTooltip(TutorialTooltip tooltip)
    {
        tooltip.Hide();
    }

    public virtual IEnumerator WaitIdleTime()
    {
        yield return WaitSeconds(TutorialController.DEFAULT_STAGE_IDLE_TIME);
    }

    public virtual IEnumerator WaitSeconds(float seconds)
    {
        float time = 0;
        while (time < seconds)
        {
            if (rendererEnabled)
            {
                time += Time.deltaTime;
            }
            yield return null;
        }
    }

    private void OnRenderingStateChanged(bool enabled)
    {
        rendererEnabled = enabled;
    }
}
