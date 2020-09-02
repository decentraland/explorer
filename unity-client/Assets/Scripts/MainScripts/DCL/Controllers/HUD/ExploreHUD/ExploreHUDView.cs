using UnityEngine;
using UnityEngine.UI;

public class ExploreHUDView : MonoBehaviour
{
    [SerializeField] internal HighlightScenesController highlightScenesController;
    [SerializeField] internal ShowHideAnimator popup;
    [SerializeField] internal Button_OnPointerDown closeButton;
    [SerializeField] internal GotoMagicButton gotoMagicButton;
    [SerializeField] internal Button_OnPointerDown togglePopupButton;
    [SerializeField] internal Color[] friendColors = null;

    public event System.Action OnClose;

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            if (!IsActive())
            {
                popup.gameObject.SetActive(true);
            }
            popup.Show();
        }
        else
        {
            popup.Hide();
            OnClose?.Invoke();
        }

    }

    public bool IsVisible()
    {
        return popup.isVisible;
    }

    public bool IsActive()
    {
        return popup.gameObject.activeSelf;
    }

    public void RefreshData()
    {
        highlightScenesController.RefreshIfNeeded();
    }

    public void Initialize(ExploreMiniMapDataController mapDataController, FriendTrackerController friendsController)
    {
        highlightScenesController.Initialize(mapDataController, friendsController);
    }
}
