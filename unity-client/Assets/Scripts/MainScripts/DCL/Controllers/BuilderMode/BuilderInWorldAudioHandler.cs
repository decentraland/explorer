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

    AudioEvent eventAssetSpawn;
    AudioEvent eventAssetPlace;

    private void Start() {
        eventAssetSpawn = audioContainer.GetEvent("BuilderAssetSpawn");
        eventAssetPlace = audioContainer.GetEvent("BuilderAssetPlace");

        creatorController.OnSceneObjectPlaced += OnAssetSpawn;
    }

    private void OnDestroy() {
        creatorController.OnSceneObjectPlaced -= OnAssetSpawn;
    }

    void OnAssetSpawn() {
        eventAssetSpawn.Play();
    }

    void OnAssetPlace() {
        eventAssetPlace.Play();
    }
}
