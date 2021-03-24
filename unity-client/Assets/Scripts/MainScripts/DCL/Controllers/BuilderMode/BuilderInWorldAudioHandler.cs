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

    AudioEvent eventAssetSpawn;
    AudioEvent eventAssetPlace;

    BuilderInWorldMode[] builderInWorldModes;

    private void Start() {
        eventAssetSpawn = audioContainer.GetEvent("BuilderAssetSpawn");
        eventAssetPlace = audioContainer.GetEvent("BuilderAssetPlace");

        creatorController.OnSceneObjectPlaced += OnAssetSpawn;

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

    void OnAssetSelect() {
        AudioScriptableObjects.inputFieldUnfocus.Play(true);
    }
}
