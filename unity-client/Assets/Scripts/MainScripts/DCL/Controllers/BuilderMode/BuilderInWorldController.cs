using Builder;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Tutorial;
using UnityEngine;
using Environment = DCL.Environment;

public class BuilderInWorldController : MonoBehaviour
{
    [Header("Activation of Feature")]
    public bool activeFeature = false;
    public bool bypassLandOwnershipCheck = false;

    [Header("DesignVariables")]
    [SerializeField]
    private float distanceToDisableBuilderInWorld = 45f;

    [Header("Scene References")]
    public GameObject cameraParentGO;

    public GameObject cursorGO;
    public InputController inputController;
    public GameObject[] groundVisualsGO;

    [Header("Prefab References")]
    public BIWOutlinerController outlinerController;
    public BIWInputHandler bIWInputHandler;
    public BIWPublishController biwPublishController;
    public BIWCreatorController biwCreatorController;
    public BIWModeController biwModeController;
    public BIWFloorHandler biwFloorHandler;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public ActionController actionController;
    public BuilderInWorldBridge builderInWorldBridge;

    [Header("Build Modes")]
    public BuilderInWorldGodMode editorMode;
    public LayerMask layerToRaycast;

    private ParcelScene sceneToEdit;

    [Header("Project References")]
    public Material skyBoxMaterial;

    [Header("Loading")]
    public BuilderInWorldLoadingView initialLoadingView;

    [HideInInspector]
    public bool isBuilderInWorldActivated = false;

    private GameObject editionGO;
    private GameObject undoGO;
    private GameObject snapGO;
    private GameObject freeMovementGO;

    private int checkerInsideSceneOptimizationCounter = 0;

    private string sceneToEditId;

    private const float RAYCAST_MAX_DISTANCE = 10000f;

    private bool catalogAdded = false;
    private bool sceneReady = false;
    private bool isInit = false;
    private Material previousSkyBoxMaterial;
    private Vector3 parcelUnityMiddlePoint;

    internal IBuilderInWorldLoadingController initialLoadingController;

    private void Awake() { BIWCatalogManager.Init(); }

    void Start()
    {
        KernelConfig.i.EnsureConfigInitialized().Then(config =>  EnableFeature(config.features.enableBuilderInWorld));
        KernelConfig.i.OnChange += OnKernelConfigChanged;
    }

    private void OnDestroy()
    {
        KernelConfig.i.OnChange -= OnKernelConfigChanged;

        if (HUDController.i.builderInWorldInititalHud != null)
            HUDController.i.builderInWorldInititalHud.OnEnterEditMode -= TryStartEnterEditMode;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnTutorialAction -= StartTutorial;
            HUDController.i.builderInWorldMainHud.OnLogoutAction -= ExitEditMode;
        }

        if (initialLoadingController != null)
        {
            initialLoadingController.OnCancelLoading -= ExitEditMode;
            initialLoadingController.Dispose();
        }

        BuilderInWorldNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;
        CleanItems();
    }

    private void Update()
    {
        if (!isBuilderInWorldActivated)
            return;

        if (checkerInsideSceneOptimizationCounter >= 60)
        {
            if (Vector3.Distance(DCLCharacterController.i.characterPosition.unityPosition, parcelUnityMiddlePoint) >= distanceToDisableBuilderInWorld)
                ExitEditMode();
            checkerInsideSceneOptimizationCounter = 0;
        }
        else
        {
            checkerInsideSceneOptimizationCounter++;
        }
    }

    private void OnNFTUsageChange()
    {
        HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
    }

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous) { EnableFeature(current.features.enableBuilderInWorld); }

    private void EnableFeature(bool enable)
    {
        activeFeature = enable;
        if (enable)
        {
            bypassLandOwnershipCheck = true;
            Init();
        }
    }

    private void CatalogReceived(string catalogJson)
    {
        AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);
        CatalogLoaded();
    }

    public void CatalogLoaded()
    {
        catalogAdded = true;
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
        StartEditMode();
    }

    public void Init()
    {
        if (isInit)
            return;

        isInit = true;

        InitGameObjects();

        HUDConfiguration hudConfig = new HUDConfiguration();
        hudConfig.active = true;
        hudConfig.visible = false;
        HUDController.i.CreateHudElement<BuildModeHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_MAIN);
        HUDController.i.CreateHudElement<BuilderInWorldInititalHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_INITIAL);
        HUDController.i.builderInWorldMainHud.Initialize();

        HUDController.i.builderInWorldInititalHud.OnEnterEditMode += TryStartEnterEditMode;
        HUDController.i.builderInWorldMainHud.OnTutorialAction += StartTutorial;
        HUDController.i.builderInWorldMainHud.OnLogoutAction += ExitEditMode;

        ConfigureLoadingController();
        InitControllers();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        CoroutineStarter.Start(BuilderInWorldUtils.MakeGetCall(BuilderInWorldSettings.BASE_URL_ASSETS_PACK, CatalogReceived));
        BuilderInWorldNFTController.i.Initialize();
        BuilderInWorldNFTController.i.OnNFTUsageChange += OnNFTUsageChange;
    }

    private void ConfigureLoadingController()
    {
        initialLoadingController = new BuilderInWorldLoadingController();
        initialLoadingController.Initialize(initialLoadingView);
        initialLoadingController.OnCancelLoading += ExitEditMode;
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

    private void StartTutorial() { TutorialController.i.SetBuilderInWorldTutorialEnabled(); }

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
        biwCreatorController?.Clean();
    }

    [ContextMenu("Activate feature")]
    public void ActivateFeature()
    {
        activeFeature = true;
        HUDController.i.taskbarHud.SetBuilderInWorldStatus(activeFeature);
    }

    public void ChangeFeatureActivationState()
    {
        if (!activeFeature)
            return;

        if (isBuilderInWorldActivated)
            ExitEditMode();
        else
            TryStartEnterEditMode();
    }

    public DCLBuilderInWorldEntity GetEntityOnPointer()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(biwModeController.GetMousePosition());
        float distanceToSelect = biwModeController.GetMaxDistanceToSelectEntities();

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

        float currentDistance = 9999;
        VoxelEntityHit voxelEntityHit = null;

        hits = Physics.RaycastAll(ray, RAYCAST_MAX_DISTANCE, layerToRaycast);

        foreach (RaycastHit hit in hits)
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                DCLBuilderInWorldEntity entityToCheck = builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);

                if (entityToCheck == null)
                    continue;

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

    private bool UserHasPermissionOnParcelScene(ParcelScene scene)
    {
        if (bypassLandOwnershipCheck)
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

    private void CheckEnterEditMode()
    {
        if (catalogAdded && sceneReady)
            EnterEditMode();
    }

    public void TryStartEnterEditMode() { TryStartEnterEditMode(true); }

    public void TryStartEnterEditMode(bool activateCamera)
    {
        if (sceneToEditId != null)
            return;

        FindSceneToEdit();

        if (!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            Notification.Model notificationModel = new Notification.Model();
            notificationModel.message = "You don't have permissions to operate this land";
            notificationModel.type = NotificationFactory.Type.GENERIC;
            HUDController.i.notificationHud.ShowNotification(notificationModel);
            return;
        }

        initialLoadingController.Show();

        //Note (Adrian) this should handle different when we have the full flow of the feature
        if (activateCamera)
            editorMode.ActivateCamera(sceneToEdit);

        if (catalogAdded)
            StartEditMode();
    }

    private void StartEditMode()
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

        inputController.isBuildModeActivate = true;

        FindSceneToEdit();

        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);
        parcelUnityMiddlePoint = BuilderInWorldUtils.CalculateUnityMiddlePoint(sceneToEdit);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.SetVisibility(true);
            HUDController.i.builderInWorldMainHud.SetParcelScene(sceneToEdit);
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
            HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        }

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);

        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;

        StartBiwControllers();
        Environment.i.world.sceneController.ActivateBuilderInWorldEditScene();

        if (IsNewScene())
        {
            SetupNewScene();
            biwFloorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
            biwFloorHandler.OnAllParcelsFloorLoaded += OnAllParcelsFloorLoaded;
        }
        else
        {
            initialLoadingController.Hide();
            inputController.isInputActive = true;
        }

        isBuilderInWorldActivated = true;

        foreach (var groundVisual in groundVisualsGO)
        {
            groundVisual.SetActive(false);
        }
        previousSkyBoxMaterial = RenderSettings.skybox;
        RenderSettings.skybox = skyBoxMaterial;
    }

    private void OnAllParcelsFloorLoaded()
    {
        biwFloorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide();
        inputController.isInputActive = true;
    }

    public void ExitEditMode()
    {
        builderInWorldBridge.SaveSceneState(sceneToEdit);
        biwFloorHandler.OnAllParcelsFloorLoaded -= OnAllParcelsFloorLoaded;
        initialLoadingController.Hide(true);

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        inputController.isInputActive = true;
        inputController.isBuildModeActivate = false;
        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);

        sceneToEdit.SetEditMode(false);

        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;

        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.ClearEntityList();
            HUDController.i.builderInWorldMainHud.SetVisibility(false);
        }

        Environment.i.world.sceneController.DeactivateBuilderInWorldEditScene();
        ExitBiwControllers();

        foreach (var groundVisual in groundVisualsGO)
        {
            groundVisual.SetActive(true);
        }

        isBuilderInWorldActivated = false;
        RenderSettings.skybox = previousSkyBoxMaterial;
    }

    public void StartBiwControllers()
    {
        biwModeController.EnterEditMode(sceneToEdit);
        builderInWorldEntityHandler.EnterEditMode(sceneToEdit);
        biwFloorHandler.EnterEditMode(sceneToEdit);
        biwCreatorController.EnterEditMode(sceneToEdit);
        biwPublishController.EnterEditMode(sceneToEdit);
        bIWInputHandler.EnterEditMode(sceneToEdit);
        outlinerController.EnterEditMode(sceneToEdit);
    }

    public void ExitBiwControllers()
    {
        biwModeController.ExitEditMode();
        builderInWorldEntityHandler.ExitEditMode();
        biwFloorHandler.ExitEditMode();
        biwCreatorController.ExitEditMode();
        biwPublishController.ExitEditMode();
        bIWInputHandler.ExitEditMode();
        outlinerController.ExitEditMode();
    }

    public bool IsNewScene() { return sceneToEdit.entities.Count <= 0; }

    public void SetupNewScene() { biwFloorHandler.CreateDefaultFloor(); }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position) { ExitEditMode(); }

    public void FindSceneToEdit()
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