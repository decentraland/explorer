using UnityEngine;
using UnityEngine.UI;

internal class ExploreHUDView : MonoBehaviour
{
    [SerializeField] internal HighlightScenesController highlightScenesController;
    [SerializeField] ShowHideAnimator showHideAnimator;
    [SerializeField] internal Button closeButton;

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            if (!IsActive())
            {
                gameObject.SetActive(true);
            }
            showHideAnimator.Show();
        }
        else
        {
            showHideAnimator.Hide();
        }

    }

    public bool IsVisible()
    {
        return showHideAnimator.isVisible;
    }

    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    public void RefreshData()
    {
        highlightScenesController.RefreshIfNeeded();
    }

    public void Initialize()
    {
        highlightScenesController.Initialize();
    }
}
