using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderInWorldAudioHandler : MonoBehaviour
{
    const float MUSIC_DELAY_TIME_ON_START = 4f;
    const float MUSIC_FADE_OUT_TIME_ON_EXIT = 5f;
    const float MUSIC_FADE_OUT_TIME_ON_TUTORIAL = 3f;

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

        entityHandler.OnEntityDeselected += OnAssetDeselect;
        entityHandler.OnEntitySelected += OnAssetSelect;
    }

    private void RemoveListeners()
    {
        creatorController.OnCatalogItemPlaced -= OnAssetSpawn;
        entityHandler.OnDeleteSelectedEntities -= OnAssetDelete;
        modeController.OnChangedEditModeState -= OnChangedEditModeState;
        DCL.Environment.i.world.sceneBoundsChecker.OnEntityBoundsCheckerStatusChanged -= OnEntityBoundsCheckerStatusChanged;

        DCL.Tutorial.TutorialController.i.OnTutorialEnabled -= OnTutorialEnabled;
        DCL.Tutorial.TutorialController.i.OnTutorialDisabled -= OnTutorialDisabled;

        entityHandler.OnEntityDeselected -= OnAssetDeselect;
        entityHandler.OnEntitySelected -= OnAssetSelect;
    }

    private void OnEnterEditMode() { CoroutineStarter.Start(StartBuilderMusic()); }

    private void OnExitEditMode()
    {
        eventBuilderExit.Play();
        CoroutineStarter.Start(eventBuilderMusic.FadeOut(MUSIC_FADE_OUT_TIME_ON_EXIT));
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

    private void OnTutorialEnabled()
    {
        if (inWorldController.isBuilderInWorldActivated)
            CoroutineStarter.Start(eventBuilderMusic.FadeOut(MUSIC_FADE_OUT_TIME_ON_TUTORIAL));
    }

    private void OnTutorialDisabled()
    {
        if (inWorldController.isBuilderInWorldActivated)
            CoroutineStarter.Start(StartBuilderMusic());
    }

    private IEnumerator StartBuilderMusic()
    {
        yield return new WaitForSeconds(MUSIC_DELAY_TIME_ON_START);

        if (inWorldController.isBuilderInWorldActivated)
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