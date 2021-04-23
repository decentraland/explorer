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
    AudioEvent eventBuilderOutOfBounds;
    [SerializeField]
    AudioEvent eventBuilderOutOfBoundsPlaced;
    [SerializeField]
    AudioEvent eventAssetDelete;
    [SerializeField]
    AudioEvent eventBuilderExit;
    [SerializeField]
    AudioEvent eventBuilderMusic;

    private BuilderInWorldMode[] builderInWorldModes;
    private List<string> entitiesOutOfBounds = new List<string>();

    private void Start()
    {
        inWorldController.OnEnterEditMode += OnEnterEditMode;
        inWorldController.OnExitEditMode += OnExitEditMode;

        AddListeners();
    }

    private void OnDestroy()
    {
        inWorldController.OnEnterEditMode -= OnEnterEditMode;
        inWorldController.OnExitEditMode -= OnExitEditMode;

        RemoveListeners();
    }

    private void AddListeners()
    {
        creatorController.OnCatalogItemPlaced += OnAssetSpawn;
        entityHandler.OnDeleteSelectedEntities += OnAssetDelete;
        modeController.OnChangedEditModeState += OnChangedEditModeState;
        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged += OnEntityBoundsCheckerStatusChanged;

        DCL.Tutorial.TutorialController.i.OnTutorialEnabled += OnTutorialEnabled;
        DCL.Tutorial.TutorialController.i.OnTutorialDisabled += OnTutorialDisabled;

        builderInWorldModes = builderInWorldModesParent.GetComponentsInChildren<BuilderInWorldMode>(true);
        for (int i = 0; i < builderInWorldModes.Length; i++)
        {
            builderInWorldModes[i].OnEntityDeselected += OnAssetDeselect;
            builderInWorldModes[i].OnEntitySelected += OnAssetSelect;
        }
    }

    private void RemoveListeners()
    {
        creatorController.OnCatalogItemPlaced -= OnAssetSpawn;
        entityHandler.OnDeleteSelectedEntities -= OnAssetDelete;
        modeController.OnChangedEditModeState -= OnChangedEditModeState;
        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged -= OnEntityBoundsCheckerStatusChanged;

        DCL.Tutorial.TutorialController.i.OnTutorialEnabled -= OnTutorialEnabled;
        DCL.Tutorial.TutorialController.i.OnTutorialDisabled -= OnTutorialDisabled;

        for (int i = 0; i < builderInWorldModes.Length; i++)
        {
            builderInWorldModes[i].OnEntityDeselected -= OnAssetDeselect;
            builderInWorldModes[i].OnEntitySelected -= OnAssetSelect;
        }
    }

    private void OnEnterEditMode() { StartCoroutine(StartBuilderMusic()); }

    private void OnExitEditMode()
    {
        eventBuilderExit.Play();
        StartCoroutine(eventBuilderMusic.FadeOut(5f));
    }

    private void OnAssetSpawn() { eventAssetSpawn.Play(); }

    private void OnAssetDelete(List<DCLBuilderInWorldEntity> entities)
    {
        foreach (DCLBuilderInWorldEntity deletedEntity in entities)
        {
            if (entitiesOutOfBounds.Contains(deletedEntity.rootEntity.entityId))
            {
                entitiesOutOfBounds.Remove(deletedEntity.rootEntity.entityId);
            }
        }

        eventAssetDelete.Play();
    }

    private void OnAssetSelect() { AudioScriptableObjects.inputFieldUnfocus.Play(true); }

    private void OnAssetDeselect(DCLBuilderInWorldEntity entity, bool assetIsNew)
    {
        if (assetIsNew || modeController.GetCurrentStateMode() == BIWModeController.EditModeState.FirstPerson)
        {
            eventAssetPlace.Play();

            if (entitiesOutOfBounds.Contains(entity.rootEntity.entityId))
            {
                eventBuilderOutOfBoundsPlaced.Play();
            }
        }
    }

    private void OnTutorialEnabled() { StartCoroutine(eventBuilderMusic.FadeOut(3f)); }

    private void OnTutorialDisabled()
    {
        if (modeController.GetCurrentStateMode() != BIWModeController.EditModeState.Inactive)
            StartCoroutine(StartBuilderMusic());
    }

    private IEnumerator StartBuilderMusic()
    {
        yield return new WaitForSeconds(4f);

        if (modeController.GetCurrentStateMode() != BIWModeController.EditModeState.Inactive)
            eventBuilderMusic.Play();
    }

    private void OnChangedEditModeState(BIWModeController.EditModeState previous, BIWModeController.EditModeState current)
    {
        if (previous != BIWModeController.EditModeState.Inactive)
        {
            switch (current)
            {
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

    private void OnEntityBoundsCheckerStatusChanged(DCL.Models.IDCLEntity entity, bool isInsideBoundaries)
    {
        if (!isInsideBoundaries)
        {
            if (!entitiesOutOfBounds.Contains(entity.entityId))
            {
                entitiesOutOfBounds.Add(entity.entityId);
                eventBuilderOutOfBounds.Play();
            }
        }
        else
        {
            if (entitiesOutOfBounds.Contains(entity.entityId))
            {
                entitiesOutOfBounds.Remove(entity.entityId);
            }
        }
    }
}