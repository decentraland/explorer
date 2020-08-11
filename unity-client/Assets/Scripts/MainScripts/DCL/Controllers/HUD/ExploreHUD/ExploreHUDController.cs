using UnityEngine;
using System;
using DCL.Helpers;
using DCL.Interface;

public class ExploreHUDController : IHUD
{
    internal ExploreHUDView view;

    InputAction_Trigger toggleExploreTrigger;

    ExploreMiniMapDataController miniMapDataController = new ExploreMiniMapDataController();
    ExploreFriendsController friendsController = new ExploreFriendsController();

    public event Action OnToggleTriggered;

    public ExploreHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ExploreHUD")).GetComponent<ExploreHUDView>();
        view.name = "_ExploreHUD";
        view.gameObject.SetActive(false);

        view.Initialize(miniMapDataController, friendsController);

        toggleExploreTrigger = Resources.Load<InputAction_Trigger>("ToggleExploreHud");
        toggleExploreTrigger.OnTriggered += OnToggleActionTriggered;

        view.closeButton.onPointerDown += () =>
        {
            if (view.IsVisible())
            {
                toggleExploreTrigger.RaiseOnTriggered();
            }
        };

        view.gotoMagicButton.OnGotoMagicPressed += GoToMagic;
        BaseSceneCellView.OnJumpIn += OnJumpIn;
    }

    public void SetVisibility(bool visible)
    {
        if (view == null)
        {
            return;
        }

        if (visible && !view.IsActive())
        {
            Utils.UnlockCursor();
            view.RefreshData();
        }

        view.SetVisibility(visible);
    }

    public void Dispose()
    {
        miniMapDataController.Dispose();
        friendsController.Dispose();

        toggleExploreTrigger.OnTriggered -= OnToggleActionTriggered;
        BaseSceneCellView.OnJumpIn -= OnJumpIn;

        if (view != null)
        {
            view.gotoMagicButton.OnGotoMagicPressed -= GoToMagic;
            GameObject.Destroy(view.gameObject);
        }
    }

    void OnToggleActionTriggered(DCLAction_Trigger action)
    {
        if (view)
        {
            OnToggleTriggered?.Invoke();
        }
    }

    void OnJumpIn(Vector2Int coords, string serverName, string layerName)
    {
        if (view.IsVisible())
        {
            toggleExploreTrigger.RaiseOnTriggered();
        }

        if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(layerName))
        {
            WebInterface.GoTo(coords.x, coords.y);
        }
        else
        {
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);
        }
    }

    void GoToMagic()
    {
        if (view.IsVisible())
        {
            toggleExploreTrigger.RaiseOnTriggered();
        }

        WebInterface.GoToMagic();
    }
}
