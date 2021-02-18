using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWInputHandler : BIWController
{
    [Header("Design variables")]
    public float msBetweenInputInteraction = 200;

    [Header("References")]
    public BuilderInWorldController builderInWorldController;
    public ActionController actionController;
    public BIWModeController biwModeController;
    public BuilderInWorldInputWrapper builderInputWrapper;
    public BIWOutlinerController outlinerController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;

    [Header("InputActions")]
    [SerializeField]
    internal InputAction_Trigger editModeChangeInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleRedoActionInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleUndoActionInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleSnapModeInputAction;

    [SerializeField]
    internal InputAction_Hold multiSelectionInputAction;

    private InputAction_Hold.Started multiSelectionStartDelegate;
    private InputAction_Hold.Finished multiSelectionFinishedDelegate;

    private InputAction_Trigger.Triggered redoDelegate;
    private InputAction_Trigger.Triggered undoDelegate;

    [HideInInspector]
    public bool isEditModeActivated = false,
    isMultiSelectionActive = false,
    isAdvancedModeActive = true;

    private float nexTimeToReceiveInput;

    void Start()
    {
        editModeChangeInputAction.OnTriggered += OnEditModeChangeAction;

        redoDelegate = (action) => RedoAction();
        undoDelegate = (action) => UndoAction();

        toggleRedoActionInputAction.OnTriggered += redoDelegate;
        toggleUndoActionInputAction.OnTriggered += undoDelegate;

        multiSelectionStartDelegate = (action) => StartMultiSelection();
        multiSelectionFinishedDelegate = (action) => EndMultiSelection();

        builderInputWrapper.OnMouseClick += MouseClick;
        biwModeController.OnInputDone += InputDone;

        multiSelectionInputAction.OnStarted += multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished += multiSelectionFinishedDelegate;
    }
    private void OnDestroy()
    {
        editModeChangeInputAction.OnTriggered -= OnEditModeChangeAction;

        toggleRedoActionInputAction.OnTriggered -= redoDelegate;
        toggleUndoActionInputAction.OnTriggered -= undoDelegate;

        multiSelectionInputAction.OnStarted -= multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished -= multiSelectionFinishedDelegate;

        builderInputWrapper.OnMouseClick -= MouseClick;
        biwModeController.OnInputDone -= InputDone;
    }

    protected override void FrameUpdate()
    {
        base.FrameUpdate();

        if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
        {
            if (Utils.isCursorLocked || isAdvancedModeActive)
                CheckEditModeInput();
            biwModeController.CheckInput();
        }
    }

    void CheckEditModeInput()
    {
        if (!builderInWorldEntityHandler.IsAnyEntitySelected() || isMultiSelectionActive)
        {
            outlinerController.CheckOutline();
        }

        if (builderInWorldEntityHandler.IsAnyEntitySelected())
        {
            biwModeController.CheckInputSelectedEntities();
        }
    }

    void StartMultiSelection()
    {
        isMultiSelectionActive = true;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        biwModeController.StartMultiSelection();
    }

    void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        biwModeController.EndMultiSelection();
        outlinerController.CancelUnselectedOutlines();
    }

    void MouseClick(int buttonID, Vector3 position)
    {
        if (!isEditModeActivated) return;

        if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
        {
            if (Utils.isCursorLocked || isAdvancedModeActive)
            {
                if (buttonID == 0)
                {
                    MouseClickDetected();
                    InputDone();
                    return;
                }

                outlinerController.CheckOutline();
            }
        }
    }

    private void OnEditModeChangeAction(DCLAction_Trigger action)
    {
        builderInWorldController.ChangeFeatureActivationState();
    }

    void RedoAction()
    {
        actionController.TryToRedoAction();
        InputDone();
    }

    void UndoAction()
    {
        InputDone();

        if (biwModeController.ShouldCancelUndoAction())
            return;

        actionController.TryToUndoAction();
    }
    void MouseClickDetected()
    {
        DCLBuilderInWorldEntity entityToSelect = builderInWorldController.GetEntityOnPointer();
        if (entityToSelect != null)
        {
            builderInWorldEntityHandler.EntityClicked(entityToSelect);
        }
        else if (!isMultiSelectionActive)
        {
            builderInWorldEntityHandler.DeselectEntities();
        }
    }

    void InputDone()
    {
        nexTimeToReceiveInput = Time.timeSinceLevelLoad + msBetweenInputInteraction / 1000;
    }
}
