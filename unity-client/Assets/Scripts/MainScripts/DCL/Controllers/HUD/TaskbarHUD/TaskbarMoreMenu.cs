using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarMoreMenu : MonoBehaviour
{
    [SerializeField] internal ShowHideAnimator moreMenuAnimator;

    [Header("Collapse Button Config")]
    [SerializeField] internal Button collapseBarButton;
    [SerializeField] internal TMP_Text collapseBarButtonText;
    [SerializeField] internal GameObject collapseIcon;
    [SerializeField] internal GameObject collapseText;
    [SerializeField] internal GameObject expandIcon;
    [SerializeField] internal GameObject expandText;

    public void Initialize(TaskbarHUDView view)
    {
        collapseBarButton.gameObject.SetActive(true);

        collapseBarButton.onClick.AddListener(() =>
        {
            view.ShowBar(!view.isBarVisible);
            ShowMoreMenu(false);

            collapseIcon.SetActive(view.isBarVisible);
            collapseText.SetActive(view.isBarVisible);
            expandIcon.SetActive(!view.isBarVisible);
            expandText.SetActive(!view.isBarVisible);

            view.moreButton.SetToggleState(false);
        });
    }

    internal void ShowMoreMenu(bool visible, bool instant = false)
    {
        if (visible)
            moreMenuAnimator.Show(instant);
        else
            moreMenuAnimator.Hide(instant);
    }
}
