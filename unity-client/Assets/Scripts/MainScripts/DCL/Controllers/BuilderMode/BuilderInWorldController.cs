using Builder;
using Builder.Gizmos;
using Builder.MeshLoadIndicator;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Helpers.NFT;
using DCL.Interface;
using DCL.Models;
using DCL.Tutorial;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using Environment = DCL.Environment;

public class BuilderInWorldController : MonoBehaviour
{
    [Header("Activation of Feature")]
    public bool activeFeature = false;
    public bool byPassLandOwnershipCheck = false;

    [Header("Design variables")]
    public float distanceLimitToSelectObjects = 50;

    [Header("Scene References")]
    public GameObject cameraParentGO;

    public GameObject cursorGO;
    public InputController inputController;
    public PlayerAvatarController avatarRenderer;

    [Header("Prefab References")]
    public BIWOutlinerController outlinerController;
    public BIWInputHandler bIWInputHandler;
    public BIWPublishController biwPublishController;
    public BIWCreatorController biwCreatorController;
    public BIWModeController biwModeController;
    public BIWFloorHandler biwFloorHandler;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public BuilderInWorldInputWrapper builderInputWrapper;
    public DCLBuilderGizmoManager gizmoManager;
    public ActionController actionController;
    public BuilderInWorldBridge builderInWorldBridge;

    [Header("Build Modes")]
    public BuilderInWorldGodMode editorMode;

    public LayerMask layerToRaycast;

    [HideInInspector]
    public ParcelScene sceneToEdit;

    [HideInInspector]
    public bool isEditModeActivated = false,
        isMultiSelectionActive = false,
        isAdvancedModeActive = true;


    private GameObject editionGO;
    private GameObject undoGO, snapGO, freeMovementGO;

    private int checkerInsideSceneOptimizationCounter = 0;

    private string sceneToEditId;

    private const float RAYCAST_MAX_DISTANCE = 10000f;

    private bool catalogAdded = false;
    private bool sceneReady = false;
    private bool isTestMode = false;

    private void Awake()
    {
        BIWCatalogManager.Init();
    }

    void Start()
    {
        KernelConfig.i.EnsureConfigInitialized().Then(config => activeFeature = config.features.enableBuilderInWorld);
        KernelConfig.i.OnChange += OnKernelConfigChanged;

        InitGameObjects();

        HUDConfiguration hudConfig = new HUDConfiguration();
        hudConfig.active = true;
        hudConfig.visible = false;
        HUDController.i.CreateHudElement<BuildModeHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_MAIN);
        HUDController.i.CreateHudElement<BuilderInWorldInititalHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_INITIAL);

        HUDController.i.builderInWorldInititalHud.OnEnterEditMode += TryStartEnterEditMode;
        HUDController.i.builderInWorldMainHud.OnStopInput += StopInput;
        HUDController.i.builderInWorldMainHud.OnResumeInput += ResumeInput;

        HUDController.i.builderInWorldMainHud.OnChangeModeAction += ChangeAdvanceMode;
        HUDController.i.builderInWorldMainHud.OnResetAction += ResetScaleAndRotation;

        HUDController.i.builderInWorldMainHud.OnTutorialAction += StartTutorial;
        HUDController.i.builderInWorldMainHud.OnLogoutAction += ExitEditMode;

        BuilderInWorldNFTController.i.OnNFTUsageChange += OnNFTUsageChange;

        InitControllers();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        if (!isTestMode)
        {
            ExternalCallsController.i.GetContentAsString(BuilderInWorldSettings.BASE_URL_ASSETS_PACK, CatalogReceived);
            BuilderInWorldNFTController.i.Initialize();
        }
    }

    private void OnDestroy()
    {
        KernelConfig.i.OnChange -= OnKernelConfigChanged;

        if(HUDController.i.builderInWorldInititalHud != null)
            HUDController.i.builderInWorldInititalHud.OnEnterEditMode -= TryStartEnterEditMode;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnStopInput -= StopInput;
            HUDController.i.builderInWorldMainHud.OnResumeInput -= ResumeInput;

            HUDController.i.builderInWorldMainHud.OnChangeModeAction -= ChangeAdvanceMode;
            HUDController.i.builderInWorldMainHud.OnResetAction -= ResetScaleAndRotation;
        
            HUDController.i.builderInWorldMainHud.OnTutorialAction -= StartTutorial;
   
            HUDController.i.builderInWorldMainHud.OnLogoutAction -= ExitEditMode;
        }

        BuilderInWorldNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;

        CleanItems();

    }

    private void Update()
    {
        if (!isEditModeActivated) return;

        if (checkerInsideSceneOptimizationCounter >= 60)
        {

            if (!sceneToEdit.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
                ExitEditMode();
            checkerInsideSceneOptimizationCounter = 0;
        }
        else
        {
            checkerInsideSceneOptimizationCounter++;
        }
    }

    public void SetTestMode()
    {
        isTestMode = true;
    }

    void OnNFTUsageChange()
    {
        HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
    }

    void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
    {
        EnableFeature(current.features.enableBuilderInWorld);
    }

    void EnableFeature(bool enable)
    {
        activeFeature = enable;
    }

    void CatalogReceived(string catalogJson)
    {
        AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);
        CatalogLoaded();
    }

    public void CatalogLoaded()
    {
        catalogAdded = true;
        if(HUDController.i.builderInWorldMainHud != null)
           HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
        StartEnterEditMode();
    }

    void StopInput()
    {
        builderInputWrapper.StopInput();
    }

    void ResumeInput()
    {
        builderInputWrapper.ResumeInput();
    }

    public void InitGameObjects()
    {
        if (snapGO == null)
            snapGO = new GameObject("SnapGameObject");

        snapGO.transform.SetParent(transform);

        if (freeMovementGO == null)
            freeMovementGO = new GameObject("FreeMovementGO");

        freeMovementGO.transform.SetParent(cameraParentGO.transform);

        if (editionGO == null)
            editionGO = new GameObject("EditionGO");

        editionGO.transform.SetParent(cameraParentGO.transform);

        if (undoGO == null)
        {
            undoGO = new GameObject("UndoGameObject");
            undoGO.transform.SetParent(transform);
        }
    }

    public void InitControllers()
    {
        builderInWorldEntityHandler.Init();
        biwModeController.Init(editionGO, undoGO, snapGO, freeMovementGO);
        biwPublishController.Init();
        biwCreatorController.Init();
        outlinerController.Init();
        biwFloorHandler.Init();
        bIWInputHandler.Init();
    }

    void StartTutorial()
    {
        TutorialController.i.SetBuilderInWorldTutorialEnabled();
    }

    public void ChangeAdvanceMode()
    {
        biwModeController.ChangeAdvanceMode();
    }

    public void UndoEditionGOLastStep()
    {
        if (undoGO == null || editionGO == null)
            return;

        BuilderInWorldUtils.CopyGameObjectStatus(undoGO, editionGO, false, false);
    }

    public void ResetScaleAndRotation()
    {
        biwModeController.ResetScaleAndRotation();
    }

    public void CleanItems()
    {
        Destroy(undoGO);
        Destroy(snapGO);
        Destroy(editionGO);
        Destroy(freeMovementGO);

        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.Dispose();

        if (HUDController.i.builderInWorldInititalHud != null)
            HUDController.i.builderInWorldInititalHud.Dispose();

        if (Camera.main != null)
        {
            DCLBuilderOutline outliner = Camera.main.GetComponent<DCLBuilderOutline>();
            Destroy(outliner);
        }

        biwFloorHandler?.Clean();
    }

    [ContextMenu("Activate feature")]
    public void ActivateFeature()
    {
        activeFeature = true;
        HUDController.i.taskbarHud.SetBuilderInWorldStatus(activeFeature);
    }

    public void ChangeFeatureActivationState()
    {
        if (activeFeature)
        {
            if (isEditModeActivated)
            {
                ExitEditMode();
            }
            else
            {
                TryStartEnterEditMode();
            }
        }
    }

    public bool IsMultiSelectionActive() => isMultiSelectionActive;    

    public DCLBuilderInWorldEntity GetEntityOnPointer()
    {
        RaycastHit hit;
        UnityEngine.Ray ray;
        float distanceToSelect = distanceLimitToSelectObjects;
        if (!isAdvancedModeActive)
        {
            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            distanceToSelect = 9999;
        }

        if (Physics.Raycast(ray, out hit, distanceToSelect, layerToRaycast))
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                return builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);
            }
        }

        return null;
    }

    public VoxelEntityHit GetCloserUnselectedVoxelEntityOnPointer()
    {
        RaycastHit[] hits;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ;

        float currentDistance = 9999;
        VoxelEntityHit voxelEntityHit = null;

        hits = Physics.RaycastAll(ray, RAYCAST_MAX_DISTANCE, layerToRaycast);

        foreach (RaycastHit hit in hits)
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                DCLBuilderInWorldEntity entityToCheck = builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);

                if (entityToCheck == null) continue;

                Camera camera = Camera.main;

                if (!entityToCheck.IsSelected && entityToCheck.tag == BuilderInWorldSettings.VOXEL_TAG)
                {
                    if (Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position) < currentDistance)
                    {
                        voxelEntityHit = new VoxelEntityHit(entityToCheck, hit);
                        currentDistance = Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
                    }
                }
            }
        }

        return voxelEntityHit;
    }

    public void NewSceneReady(string id)
    {
        if (sceneToEditId != id)
            return;

        Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;
        sceneToEditId = null;
        sceneReady = true;
        CheckEnterEditMode();
    }

    bool UserHasPermissionOnParcelScene(ParcelScene scene)
    {
        if (byPassLandOwnershipCheck)
            return true;

        UserProfile userProfile = UserProfile.GetOwnUserProfile();
        foreach (UserProfileModel.ParcelsWithAccess parcelWithAccess in userProfile.parcelsWithAccess)
        {
            foreach (Vector2Int parcel in scene.sceneData.parcels)
            {
                if (parcel.x == parcelWithAccess.x && parcel.y == parcelWithAccess.y)
                    return true;
            }
        }
        return false;
    }

    void CheckEnterEditMode()
    {
        if (catalogAdded && sceneReady) EnterEditMode();
    }

    public void TryStartEnterEditMode()
    {
        TryStartEnterEditMode(true);
    }

    public void TryStartEnterEditMode(bool activateCamera)
    {
        if (sceneToEditId != null)
            return;

        FindSceneToEdit();

        if(!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            Notification.Model notificationModel = new Notification.Model();
            notificationModel.message = "You don't have permissions to operate this land";
            notificationModel.type = NotificationFactory.Type.GENERIC;
            HUDController.i.notificationHud.ShowNotification(notificationModel);
            return;
        }

        //Note (Adrian) this should handle different when we have the full flow of the feature
        if (activateCamera)
            editorMode.ActivateCamera(sceneToEdit);

        if (catalogAdded)
            StartEnterEditMode();
    }

    void StartEnterEditMode()
    {
        if (sceneToEdit == null)
            return;

        sceneToEditId = sceneToEdit.sceneData.id;
        inputController.isInputActive = false;

        Environment.i.world.sceneController.OnReadyScene += NewSceneReady;

        builderInWorldBridge.StartKernelEditMode(sceneToEdit);
    }

    public void EnterEditMode()
    {
        BuilderInWorldNFTController.i.ClearNFTs();

        ParcelSettings.VISUAL_LOADING_ENABLED = false;


        inputController.isInputActive = true;
        inputController.isBuildModeActivate = true;

        FindSceneToEdit();

        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.SetVisibility(true);
            HUDController.i.builderInWorldMainHud.SetParcelScene(sceneToEdit);
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
            HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        }

 

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);

        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(true);

        StartEditControllers();
        Environment.i.world.sceneController.ActivateBuilderInWorldEditScene();

 

        if (IsNewScene())
            SetupNewScene();

        isEditModeActivated = true;
    }

    public void ExitEditMode()
    {
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        inputController.isBuildModeActivate = false;
        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);
        builderInWorldEntityHandler.ExitFromEditMode();

        sceneToEdit.SetEditMode(false);
        biwModeController.ExitEditMode();


        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(false);
        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);

        avatarRenderer.SetAvatarVisibility(true);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.ClearEntityList();
            HUDController.i.builderInWorldMainHud.SetVisibility(false);
        }

        Environment.i.world.sceneController.DeactivateBuilderInWorldEditScene();

        ;
        isEditModeActivated = false;
    }

    public void StartEditControllers()
    {
        biwModeController.EnterEditMode(sceneToEdit);
        builderInWorldEntityHandler.EnterEditMode(sceneToEdit);
        biwFloorHandler.EnterEditMode(sceneToEdit);
        biwCreatorController.EnterEditMode(sceneToEdit);
        biwPublishController.EnterEditMode(sceneToEdit);
        bIWInputHandler.EnterEditMode(sceneToEdit);
        outlinerController.EnterEditMode(sceneToEdit);
    }

    public void ExitEditControllers()
    {
        biwModeController.ExitEditMode();
        builderInWorldEntityHandler.ExitEditMode();
        biwFloorHandler.ExitEditMode();
        biwCreatorController.ExitEditMode();
        biwPublishController.ExitEditMode();
        bIWInputHandler.ExitEditMode();
        outlinerController.ExitEditMode();
    }

    public bool IsNewScene()
    {
        return sceneToEdit.entities.Count <= 0;
    }

    public void SetupNewScene()
    {
        biwFloorHandler.CreateDefaultFloor();
    }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position)
    {
        ExitEditMode();
    }

    void FindSceneToEdit()
    {
        foreach (ParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
        {
            if (scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
            {
                if (sceneToEdit != null && sceneToEdit != scene)
                    actionController.Clear();
                sceneToEdit = scene;
                break;
            }
        }
    }

}