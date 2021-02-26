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

    [Header("Buttons")]
    [SerializeField] internal Button changeModeBtn;
    [SerializeField] internal Button extraBtn;
    [SerializeField] internal Button translateBtn;
    [SerializeField] internal Button rotateBtn;
    [SerializeField] internal Button scaleBtn;
    [SerializeField] internal Button resetBtn;
    [SerializeField] internal Button duplicateBtn;
    [SerializeField] internal Button deleteBtn;
    [SerializeField] internal Button logOutBtn;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleChangeCameraInputAction;
    [SerializeField] internal InputAction_Trigger toggleTranslateInputAction;
    [SerializeField] internal InputAction_Trigger toggleRotateInputAction;
    [SerializeField] internal InputAction_Trigger toggleScaleInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetInputAction;
    [SerializeField] internal InputAction_Trigger toggleDuplicateInputAction;
    [SerializeField] internal InputAction_Trigger toggleDeleteInputAction;

    [Header("Sub-Views")]
    [SerializeField] internal ExtraActionsView extraActionsView;

    internal IExtraActionsController extraActionsController;

    private void Awake()
    {
        changeModeBtn.onClick.AddListener(OnChangeModeClick);
        translateBtn.onClick.AddListener(OnTranslateClick);
        rotateBtn.onClick.AddListener(OnRotateClick);
        scaleBtn.onClick.AddListener(OnScaleClick);
        resetBtn.onClick.AddListener(OnResetClick);
        duplicateBtn.onClick.AddListener(OnDuplicateClick);
        deleteBtn.onClick.AddListener(OnDeleteClick);
        logOutBtn.onClick.AddListener(OnLogOutClick);
        extraBtn.onClick.AddListener(OnExtraClick);

        toggleChangeCameraInputAction.OnTriggered += (action) => OnChangeModeClick();
        toggleTranslateInputAction.OnTriggered += (action) => OnTranslateClick();
        toggleRotateInputAction.OnTriggered += (action) => OnRotateClick();
        toggleScaleInputAction.OnTriggered += (action) => OnScaleClick();
        toggleResetInputAction.OnTriggered += (action) => OnResetClick();
        toggleDuplicateInputAction.OnTriggered += (action) => OnDuplicateClick();
        toggleDeleteInputAction.OnTriggered += (action) => OnDeleteClick();
    }

    private void OnDestroy()
    {
        changeModeBtn.onClick.RemoveListener(OnChangeModeClick);
        translateBtn.onClick.RemoveListener(OnTranslateClick);
        rotateBtn.onClick.RemoveListener(OnRotateClick);
        scaleBtn.onClick.RemoveListener(OnScaleClick);
        resetBtn.onClick.RemoveListener(OnResetClick);
        duplicateBtn.onClick.RemoveListener(OnDuplicateClick);
        deleteBtn.onClick.RemoveListener(OnDeleteClick);
        logOutBtn.onClick.RemoveListener(OnLogOutClick);
        extraBtn.onClick.RemoveListener(OnExtraClick);

        toggleChangeCameraInputAction.OnTriggered -= (action) => OnChangeModeClick();
        toggleTranslateInputAction.OnTriggered -= (action) => OnTranslateClick();
        toggleRotateInputAction.OnTriggered -= (action) => OnRotateClick();
        toggleScaleInputAction.OnTriggered -= (action) => OnScaleClick();
        toggleResetInputAction.OnTriggered -= (action) => OnResetClick();
        toggleDuplicateInputAction.OnTriggered -= (action) => OnDuplicateClick();
        toggleDeleteInputAction.OnTriggered -= (action) => OnDeleteClick();

        if (extraActionsController != null)
            extraActionsController.Dispose();
    }

    public void ConfigureExtraActions(IExtraActionsController extraActionsController)
    {
        this.extraActionsController = extraActionsController;
        this.extraActionsController.Initialize(extraActionsView);
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
}
