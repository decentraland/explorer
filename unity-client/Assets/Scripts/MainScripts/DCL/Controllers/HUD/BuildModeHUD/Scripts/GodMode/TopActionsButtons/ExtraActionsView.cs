using System;
using UnityEngine;
using UnityEngine.UI;

public interface IExtraActionsView
{
    event Action OnControlsClicked,
                 OnHideUIClicked,
                 OnTutorialClicked;

    void OnControlsClick();
    void OnHideUIClick();
    void OnTutorialClick();
    void SetActive(bool isActive);
}

public class ExtraActionsView : MonoBehaviour, IExtraActionsView
{
    public event Action OnControlsClicked,
                        OnHideUIClicked,
                        OnTutorialClicked;

    [Header("Buttons")]
    [SerializeField] internal Button hideUIBtn;
    [SerializeField] internal Button controlsBtn;
    [SerializeField] internal Button tutorialBtn;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleUIVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleControlsVisibilityInputAction;

    private void Awake()
    {
        hideUIBtn.onClick.AddListener(OnHideUIClick);
        controlsBtn.onClick.AddListener(OnControlsClick);
        tutorialBtn.onClick.AddListener(OnTutorialClick);

        toggleUIVisibilityInputAction.OnTriggered += (action) => OnHideUIClick();
        toggleControlsVisibilityInputAction.OnTriggered += (action) => OnControlsClick();
    }

    private void OnDestroy()
    {
        hideUIBtn.onClick.RemoveListener(OnHideUIClick);
        controlsBtn.onClick.RemoveListener(OnControlsClick);
        tutorialBtn.onClick.RemoveListener(OnTutorialClick);

        toggleUIVisibilityInputAction.OnTriggered -= (action) => OnHideUIClick();
        toggleControlsVisibilityInputAction.OnTriggered -= (action) => OnControlsClick();
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void OnControlsClick()
    {
        OnControlsClicked?.Invoke();
    }

    public void OnHideUIClick()
    {
        OnHideUIClicked?.Invoke();
    }

    public void OnTutorialClick()
    {
        OnTutorialClicked?.Invoke();
    }
}
