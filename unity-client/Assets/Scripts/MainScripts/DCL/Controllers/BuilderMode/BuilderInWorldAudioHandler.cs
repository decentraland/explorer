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

        builderInWorldModes = builderInWorldModesParent.GetComponentsInChildren<BuilderInWorldMode>(true);
        for (int i = 0; i < builderInWorldModes.Length; i++) {
            builderInWorldModes[i].OnEntityPlaced += OnAssetPlace;
            builderInWorldModes[i].OnEntitySelected += OnAssetSelect;
        }
    }

    private void OnDestroy() {
        creatorController.OnSceneObjectPlaced -= OnAssetSpawn;

        for (int i = 0; i < builderInWorldModes.Length; i++) {
            builderInWorldModes[i].OnEntityPlaced -= OnAssetPlace;
            builderInWorldModes[i].OnEntitySelected -= OnAssetSelect;
        }
    }

    void OnAssetSpawn() {
        eventAssetSpawn.Play();
    }

    void OnAssetPlace() {
        eventAssetPlace.Play();
    }

    void OnAssetDelete() {
        eventAssetDelete.Play();
    }

    void OnAssetSelect() {
        AudioScriptableObjects.inputFieldUnfocus.Play(true);
    }

    void OnEnterEditMode() {
        eventBuilderEnter.Play();
    }

    void OnExitEditMode() {
        eventBuilderExit.Play();
    }
}
