using System;
using UnityEngine;
using UnityEngine.UI;

public class ExtraActionsView : MonoBehaviour
{
    internal event Action OnControlsClicked, OnHideUIClicked, OnTutorialClicked;

    [SerializeField] internal Button controlsBtn, hideUIBtn, tutorialBtn;

    private void Awake()
    {
        controlsBtn.onClick.AddListener(OnControlsClick);
        hideUIBtn.onClick.AddListener(OnHideUIClick);
        tutorialBtn.onClick.AddListener(OnTutorialClick);
    }

    private void OnDestroy()
    {
        controlsBtn.onClick.RemoveListener(OnControlsClick);
        hideUIBtn.onClick.RemoveListener(OnHideUIClick);
        tutorialBtn.onClick.RemoveListener(OnTutorialClick);
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
