using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;

public class NavmapView : MonoBehaviour
{
    [SerializeField] InputAction_Trigger toggleNavMapAction;
    [SerializeField] Button closeButton;
    [SerializeField] GameObject scrollRectGameObject;
    InputAction_Trigger.Triggered toggleNavMapDelegate;

    void Awake()
    {
        closeButton.onClick.AddListener(() => { scrollRectGameObject.SetActive(false); });

        toggleNavMapDelegate = (x) => { ToggleNavMap(); };
        toggleNavMapAction.OnTriggered += toggleNavMapDelegate;
    }

    void ToggleNavMap()
    {
        scrollRectGameObject.SetActive(!scrollRectGameObject.activeSelf);

        if (scrollRectGameObject.activeSelf)
            Utils.UnlockCursor();
        else
            Utils.LockCursor();
    }
}
