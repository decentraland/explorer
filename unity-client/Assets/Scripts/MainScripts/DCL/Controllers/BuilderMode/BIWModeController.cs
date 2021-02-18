using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWModeController : MonoBehaviour
{
    public enum EditModeState
    {
        Inactive = 0,
        FirstPerson = 1,
        Editor = 2
    }

    [Header("Scene References")]
    public GameObject cursorGO;
    public PlayerAvatarController avatarRenderer;

    [Header("References")]
    public ActionController actionController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;

    [Header("Build Modes")]
    public BuilderInWorldFirstPersonMode firstPersonMode;
    public BuilderInWorldGodMode editorMode;

    [SerializeField]
    internal InputAction_Trigger toggleSnapModeInputAction;

    public System.Action OnInputDone;

    private EditModeState currentEditModeState = EditModeState.Inactive;

    private BuilderInWorldMode currentActiveMode;

    private bool isSnapActive = true;
    private bool isAdvancedModeActive = true;

    private InputAction_Trigger.Triggered snapModeDelegate;
    private ParcelScene sceneToEdit;

    private void Start()
    {
        snapModeDelegate = (action) => ChangeSnapMode();
        toggleSnapModeInputAction.OnTriggered += snapModeDelegate;
    }

    private void OnDestroy()
    {
        toggleSnapModeInputAction.OnTriggered -= snapModeDelegate;

        firstPersonMode.OnInputDone -= InputDone;
        editorMode.OnInputDone -= InputDone;

        firstPersonMode.OnActionGenerated -= actionController.AddAction;
        editorMode.OnActionGenerated -= actionController.AddAction;
    }

    public void Init(GameObject editionGO, GameObject undoGO, GameObject snapGO, GameObject freeMovementGO)
    {
        firstPersonMode.Init(editionGO, undoGO, snapGO, freeMovementGO, builderInWorldEntityHandler.GetSelectedEntityList());
        editorMode.Init(editionGO, undoGO, snapGO, freeMovementGO, builderInWorldEntityHandler.GetSelectedEntityList());

        firstPersonMode.OnInputDone += InputDone;
        editorMode.OnInputDone += InputDone;

        firstPersonMode.OnActionGenerated += actionController.AddAction;
        editorMode.OnActionGenerated += actionController.AddAction;
    }

    public void StartEditMode(ParcelScene parcelScene)
    {
        this.sceneToEdit = parcelScene;
        if (currentActiveMode == null)
            SetBuildMode(BIWModeController.EditModeState.Editor);
    }

    public void ExitEditMode()
    {
        SetBuildMode(EditModeState.Inactive);
    }

    public BuilderInWorldMode GetCurrentMode() => currentActiveMode;


    private void InputDone()
    {
        OnInputDone?.Invoke();
    }

    public void StartMultiSelection()
    {
        currentActiveMode.StartMultiSelection();
    }

    public void EndMultiSelection()
    {
        currentActiveMode.EndMultiSelection();
    }

    public void ResetScaleAndRotation()
    {
        currentActiveMode.ResetScaleAndRotation();
    }

    public void CheckInput()
    {
        currentActiveMode?.CheckInput();
    }

    public void CheckInputSelectedEntities()
    {
        currentActiveMode.CheckInputSelectedEntities();
    }

    public bool ShouldCancelUndoAction()
    {
        return currentActiveMode.ShouldCancelUndoAction();
    }

    public void CreatedEntity(DCLBuilderInWorldEntity entity)
    {
        currentActiveMode?.CreatedEntity(entity);
    }

    public Vector3 GetModeCreationEntryPoint()
    {
        if (currentActiveMode != null)
            return currentActiveMode.GetCreatedEntityPoint();
        return Vector3.zero;
    }
    void ChangeSnapMode()
    {
        SetSnapActive(!isSnapActive);
        InputDone();
    }

    public void SetSnapActive(bool isActive)
    {
        isSnapActive = isActive;
        currentActiveMode.SetSnapActive(isActive);
    }

    public void ChangeAdvanceMode()
    {
        SetAdvanceMode(!isAdvancedModeActive);
        InputDone();
    }

    public void SetAdvanceMode(bool advanceModeActive)
    {
        if (!advanceModeActive)
        {
            SetBuildMode(EditModeState.FirstPerson);
        }
        else
        {
            SetBuildMode(EditModeState.Editor);
        }
    }

    public void SetBuildMode(EditModeState state)
    {
        if (currentActiveMode != null)
            currentActiveMode.Deactivate();
        isAdvancedModeActive = false;

        currentActiveMode = null;
        switch (state)
        {
            case EditModeState.Inactive:
                break;
            case EditModeState.FirstPerson:
                currentActiveMode = firstPersonMode;
                if (HUDController.i.builderInWorldMainHud != null)
                {
                    HUDController.i.builderInWorldMainHud.ActivateFirstPersonModeUI();
                    HUDController.i.builderInWorldMainHud.SetVisibilityOfCatalog(false);
                }
                cursorGO.SetActive(true);
                break;
            case EditModeState.Editor:
                cursorGO.SetActive(false);
                currentActiveMode = editorMode;
                isAdvancedModeActive = true;
                if (HUDController.i.builderInWorldMainHud != null)
                    HUDController.i.builderInWorldMainHud.ActivateGodModeUI();

                avatarRenderer.SetAvatarVisibility(false);
                break;
        }

        currentEditModeState = state;

        if (currentActiveMode != null)
        {
            currentActiveMode.Activate(sceneToEdit);
            currentActiveMode.SetSnapActive(isSnapActive);
            builderInWorldEntityHandler.SetActiveMode(currentActiveMode);
        }
    }
}
