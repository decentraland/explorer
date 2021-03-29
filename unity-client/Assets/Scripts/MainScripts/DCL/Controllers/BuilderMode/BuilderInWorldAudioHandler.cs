using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderInWorldAudioHandler : MonoBehaviour
{
    [SerializeField]
    AudioContainer audioContainer;

    [SerializeField]
    BIWCreatorController creatorController;

    [SerializeField]
    GameObject builderInWorldModesParent;

    [SerializeField]
    BuilderInWorldController inWorldController;

    [SerializeField]
    BuilderInWorldEntityHandler entityHandler;

    [SerializeField]
    BIWModeController modeController;

    [Header("Audio Events")]
    [SerializeField]
    AudioEvent eventAssetSpawn;
    [SerializeField]
    AudioEvent eventAssetPlace;
    [SerializeField]
    AudioEvent eventAssetDelete;
    [SerializeField]
    AudioEvent eventBuilderEnter;
    [SerializeField]
    AudioEvent eventBuilderExit;

    BuilderInWorldMode[] builderInWorldModes;

    private void Start() {
        creatorController.OnSceneObjectPlaced += OnAssetSpawn;
        inWorldController.OnEnterEditMode += OnEnterEditMode;
        inWorldController.OnExitEditMode += OnExitEditMode;
        entityHandler.OnDeleteSelectedEntities += OnAssetDelete;
        modeController.OnChangedEditModeState += OnChangedEditModeState;

        builderInWorldModes = builderInWorldModesParent.GetComponentsInChildren<BuilderInWorldMode>(true);
        for (int i = 0; i < builderInWorldModes.Length; i++) {
            builderInWorldModes[i].OnEntityDeselected += OnAssetDeselect;
            builderInWorldModes[i].OnEntitySelected += OnAssetSelect;
        }
    }

    private void OnDestroy() {
        creatorController.OnSceneObjectPlaced -= OnAssetSpawn;

        for (int i = 0; i < builderInWorldModes.Length; i++) {
            builderInWorldModes[i].OnEntityDeselected -= OnAssetDeselect;
            builderInWorldModes[i].OnEntitySelected -= OnAssetSelect;
        }
    }

    void OnAssetSpawn() {
        eventAssetSpawn.Play();
    }

    void OnAssetDelete() {
        eventAssetDelete.Play();
    }

    void OnAssetSelect() {
        AudioScriptableObjects.inputFieldUnfocus.Play(true);
    }

    void OnAssetDeselect(bool assetIsNew) {
        if (assetIsNew || modeController.GetCurrentStateMode() == BIWModeController.EditModeState.FirstPerson)
            eventAssetPlace.Play();
    }

    void OnEnterEditMode() {
        eventBuilderEnter.Play();
    }

    void OnExitEditMode() {
        eventBuilderExit.Play();
    }

    void OnChangedEditModeState(BIWModeController.EditModeState previous, BIWModeController.EditModeState current) {
        if (previous != BIWModeController.EditModeState.Inactive) {
            switch (current) {
                case BIWModeController.EditModeState.FirstPerson:
                    AudioScriptableObjects.cameraFadeIn.Play();
                    break;
                case BIWModeController.EditModeState.GodMode:
                    AudioScriptableObjects.cameraFadeOut.Play();
                    break;
                default:
                    break;
            }
        }
    }
}
