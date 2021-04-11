using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderInWorldAudioHandler : MonoBehaviour
{
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
    AudioEvent eventBuilderExit;
    [SerializeField]
    AudioEvent eventBuilderMusic;

    BuilderInWorldMode[] builderInWorldModes;

    private void Start() {
        creatorController.OnSceneObjectPlaced += OnAssetSpawn;
        inWorldController.OnEnterEditMode += OnEnterEditMode;
        inWorldController.OnExitEditMode += OnExitEditMode;
        entityHandler.OnDeleteSelectedEntities += OnAssetDelete;
        modeController.OnChangedEditModeState += OnChangedEditModeState;

        DCL.Tutorial.TutorialController.i.OnTutorialEnabled += OnTutorialEnabled;
        DCL.Tutorial.TutorialController.i.OnTutorialDisabled += OnTutorialDisabled;

        builderInWorldModes = builderInWorldModesParent.GetComponentsInChildren<BuilderInWorldMode>(true);
        for (int i = 0; i < builderInWorldModes.Length; i++) {
            builderInWorldModes[i].OnEntityDeselected += OnAssetDeselect;
            builderInWorldModes[i].OnEntitySelected += OnAssetSelect;
        }
    }

    private void OnDestroy() {
        creatorController.OnSceneObjectPlaced -= OnAssetSpawn;
        inWorldController.OnEnterEditMode -= OnEnterEditMode;
        inWorldController.OnExitEditMode -= OnExitEditMode;
        entityHandler.OnDeleteSelectedEntities -= OnAssetDelete;
        modeController.OnChangedEditModeState -= OnChangedEditModeState;

        DCL.Tutorial.TutorialController.i.OnTutorialEnabled -= OnTutorialEnabled;
        DCL.Tutorial.TutorialController.i.OnTutorialDisabled -= OnTutorialDisabled;

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
        StartCoroutine(StartBuilderMusic());
    }

    void OnExitEditMode() {
        eventBuilderExit.Play();
        StartCoroutine(eventBuilderMusic.FadeOut(5f));
    }

    IEnumerator StartBuilderMusic() {
        yield return new WaitForSeconds(4f);

        eventBuilderMusic.Play();
    }

    void OnTutorialEnabled() {
        StartCoroutine(eventBuilderMusic.FadeOut(3f));
    }

    void OnTutorialDisabled() {
        StartCoroutine(StartBuilderMusic());
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
