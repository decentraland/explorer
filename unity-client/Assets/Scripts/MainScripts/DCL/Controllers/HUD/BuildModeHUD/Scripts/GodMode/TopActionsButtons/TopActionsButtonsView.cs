using System;
using UnityEngine;
using UnityEngine.UI;

public class TopActionsButtonsView : MonoBehaviour
{
    internal event Action OnChangeModeClicked,
                          OnExtraClicked,
                          OnTranslateClicked,
                          OnRotateClicked,
                          OnScaleClicked,
                          OnResetClicked,
                          OnDuplicateClicked,
                          OnDeleteClicked,
                          OnLogOutClicked;

    [SerializeField] internal Button changeModeBtn,
                                     extraBtn,
                                     translateBtn,
                                     rotateBtn,
                                     scaleBtn,
                                     resetBtn,
                                     duplicateBtn,
                                     deleteBtn,
                                     logOutBtn;

    [SerializeField] internal ExtraActionsView extraActionsView;

    internal IExtraActionsController extraActionsController;

    private void Awake()
    {
        changeModeBtn.onClick.AddListener(OnChangeModeClick);
        extraBtn.onClick.AddListener(OnExtraClick);
        translateBtn.onClick.AddListener(OnTranslateClick);
        rotateBtn.onClick.AddListener(OnRotateClick);
        scaleBtn.onClick.AddListener(OnScaleClick);
        resetBtn.onClick.AddListener(OnResetClick);
        duplicateBtn.onClick.AddListener(OnDuplicateClick);
        deleteBtn.onClick.AddListener(OnDeleteClick);
        logOutBtn.onClick.AddListener(OnLogOutClick);
    }

    private void OnDestroy()
    {
        changeModeBtn.onClick.RemoveListener(OnChangeModeClick);
        extraBtn.onClick.RemoveListener(OnExtraClick);
        translateBtn.onClick.RemoveListener(OnTranslateClick);
        rotateBtn.onClick.RemoveListener(OnRotateClick);
        scaleBtn.onClick.RemoveListener(OnScaleClick);
        resetBtn.onClick.RemoveListener(OnResetClick);
        duplicateBtn.onClick.RemoveListener(OnDuplicateClick);
        deleteBtn.onClick.RemoveListener(OnDeleteClick);
        logOutBtn.onClick.RemoveListener(OnLogOutClick);

        if (extraActionsController != null)
            extraActionsController.Dispose();
    }

    public void OnChangeModeClick()
    {
        OnChangeModeClicked?.Invoke();
    }

    public void OnExtraClick()
    {
        OnExtraClicked?.Invoke();
    }

    public void OnTranslateClick()
    {
        OnTranslateClicked?.Invoke();
    }

    public void OnRotateClick()
    {
        OnRotateClicked?.Invoke();
    }

    public void OnScaleClick()
    {
        OnScaleClicked?.Invoke();
    }

    public void OnResetClick()
    {
        OnResetClicked?.Invoke();
    }

    public void OnDuplicateClick()
    {
        OnDuplicateClicked?.Invoke();
    }

    public void OnDeleteClick()
    {
        OnDeleteClicked?.Invoke();
    }

    public void OnLogOutClick()
    {
        OnLogOutClicked?.Invoke();
    }

    public void ConfigureExtraActions(IExtraActionsController extraActionsController)
    {
        this.extraActionsController = extraActionsController;
        this.extraActionsController.Initialize(extraActionsView);
    }
}
