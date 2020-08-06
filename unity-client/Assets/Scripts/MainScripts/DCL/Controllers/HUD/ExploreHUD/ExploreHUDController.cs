using UnityEngine;
using System;
using DCL.Helpers;

public class ExploreHUDController : IHUD
{
    internal ExploreHUDView view;

    InputAction_Trigger toggleExploreTrigger;

    public event Action OnToggleTriggered;

    public ExploreHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ExploreHUD")).GetComponent<ExploreHUDView>();
        view.name = "_ExploreHUD";
        view.gameObject.SetActive(false);
        view.Initialize();

        toggleExploreTrigger = Resources.Load<InputAction_Trigger>("ToggleExploreHud");
        toggleExploreTrigger.OnTriggered += OnToggleActionTriggered;

        view.closeButton.onClick.AddListener(() =>
        {
            if (view.IsVisible())
            {
                toggleExploreTrigger.RaiseOnTriggered();
            }
        });
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
        if (view != null)
            GameObject.Destroy(view.gameObject);

        toggleExploreTrigger.OnTriggered -= OnToggleActionTriggered;
    }

    void OnToggleActionTriggered(DCLAction_Trigger action)
    {
        if (view)
        {
            OnToggleTriggered?.Invoke();
        }
    }
}
