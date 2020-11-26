using Builder;
using Builder.Gizmos;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
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
using UnityEngine.XR;

public class BuildModeController : MonoBehaviour
{
    public enum EditModeState
    {
        Inactive = 0,
        FirstPerson = 1,
        Editor = 2
    }

    [Header("Activation of Feature")]

    public bool activeFeature = false;


    [Header("Design variables")]

    public float scaleSpeed = 0.25f;
    public float rotationSpeed = 0.5f;
    public float msBetweenInputInteraction = 200;

    public float distanceLimitToSelectObjects = 50;


    [Header("Snap variables")]
    public float snapFactor = 1f;
    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;

    public float snapDistanceToActivateMovement = 10f;


    [Header("Scene References")]
    public GameObject cameraParentGO;
    public GameObject cursorGO;
    public InputController inputController;

    [Header("Prefab References")]
    public OutlinerController outlinerController;
    public BuilderInputWrapper builderInputWrapper;
    public DCLBuilderGizmoManager gizmoManager;
    public ActionController actionController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public BuilderInWorldBridge builderInWorldBridge;

    [Header("Build Modes")]

    public BuildFirstPersonMode firstPersonMode;
    public BuildEditorMode editorMode;

    [Header("Build References")]

    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChangeInputAction;
    [SerializeField] internal InputAction_Trigger toggleCreateLastSceneObjectInputAction;
    [SerializeField] internal InputAction_Trigger toggleRedoActionInputAction;
    [SerializeField] internal InputAction_Trigger toggleUndoActionInputAction;
    [SerializeField] internal InputAction_Trigger toggleSnapModeInputAction;

    [SerializeField] internal InputAction_Hold multiSelectionInputAction;

    //Note(Adrian): This is for tutorial purposes
    public Action OnSceneObjectPlaced;

    BuildMode currentActiveMode;

    ParcelScene sceneToEdit;

    bool isEditModeActivated = false,
         isSnapActive = true,
         isMultiSelectionActive = false,
         isAdvancedModeActive = true,
         isOutlineCheckActive = true;


    GameObject editionGO;
    GameObject undoGO, snapGO, freeMovementGO;

    float nexTimeToReceiveInput;

    int outlinerOptimizationCounter = 0, checkerInsideSceneOptimizationCounter = 0;

    SceneObject lastSceneObjectCreated;

    const float RAYCAST_MAX_DISTANCE = 10000f;

    InputAction_Hold.Started multiSelectionStartDelegate;
    InputAction_Hold.Finished multiSelectionFinishedDelegate;

    InputAction_Trigger.Triggered createLastSceneObjectDelegate;
    InputAction_Trigger.Triggered redoDelegate;
    InputAction_Trigger.Triggered undoDelegate;
    InputAction_Trigger.Triggered snapModeDelegate;

    bool catalogInitializaed = false;
    bool catalogAdded = false;

    void Start()
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

        HUDConfiguration hudConfig = new HUDConfiguration();
        hudConfig.active = true;
        hudConfig.visible = false;
        HUDController.i.CreateHudElement<BuildModeHUDController>(hudConfig, HUDController.HUDElementID.BUILD_MODE);

        editModeChangeInputAction.OnTriggered += OnEditModeChangeAction;

        createLastSceneObjectDelegate = (action) => CreateLastSceneObject();
        redoDelegate = (action) => RedoAction();
        undoDelegate = (action) => UndoAction();
        snapModeDelegate = (action) => ChangeSnapMode();

        toggleCreateLastSceneObjectInputAction.OnTriggered += createLastSceneObjectDelegate;
        toggleRedoActionInputAction.OnTriggered += redoDelegate;
        toggleUndoActionInputAction.OnTriggered += undoDelegate;
        toggleSnapModeInputAction.OnTriggered += snapModeDelegate;

        multiSelectionStartDelegate = (action) => StartMultiSelection();
        multiSelectionFinishedDelegate = (action) => EndMultiSelection();

        multiSelectionInputAction.OnStarted += multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished += multiSelectionFinishedDelegate;

        HUDController.i.buildModeHud.OnStopInput += StopInput;
        HUDController.i.buildModeHud.OnResumeInput += ResumeInput;


        HUDController.i.buildModeHud.OnChangeModeAction += ChangeAdvanceMode;
        HUDController.i.buildModeHud.OnResetAction += ResetScaleAndRotation;

        HUDController.i.buildModeHud.OnSceneObjectSelected += CreateSceneObjectSelected;
        HUDController.i.buildModeHud.OnTutorialAction += StartTutorial;
        HUDController.i.buildModeHud.OnPublishAction += PublishScene;

        builderInputWrapper.OnMouseClick += MouseClick;

        builderInWorldEntityHandler.Init();
        InitEditModes();


        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        if (!catalogInitializaed)
        {
            AssetCatalog.sceneAssetPackCatalog.GetValues();
            ExternalCallsController.i.GetContentAsString(BuilderInWorldSettings.BASE_URL_ASSETS_PACK, CatalogReceived);
            catalogInitializaed = true;
        }
    }

    private void OnDestroy()
    {
        editModeChangeInputAction.OnTriggered -= OnEditModeChangeAction;

        toggleCreateLastSceneObjectInputAction.OnTriggered -= createLastSceneObjectDelegate;
        toggleRedoActionInputAction.OnTriggered -= redoDelegate;
        toggleUndoActionInputAction.OnTriggered -= undoDelegate;
        toggleSnapModeInputAction.OnTriggered -= snapModeDelegate;

        multiSelectionInputAction.OnStarted -= multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished -= multiSelectionFinishedDelegate;

        HUDController.i.buildModeHud.OnStopInput -= StopInput;
        HUDController.i.buildModeHud.OnResumeInput -= ResumeInput;

        HUDController.i.buildModeHud.OnChangeModeAction -= ChangeAdvanceMode;
        HUDController.i.buildModeHud.OnResetAction -= ResetScaleAndRotation;

        HUDController.i.buildModeHud.OnSceneObjectSelected -= CreateSceneObjectSelected;
        HUDController.i.buildModeHud.OnTutorialAction -= StartTutorial;
        HUDController.i.buildModeHud.OnPublishAction -= PublishScene;


        builderInputWrapper.OnMouseClick -= MouseClick;

        firstPersonMode.OnInputDone -= InputDone;
        editorMode.OnInputDone -= InputDone;

        firstPersonMode.OnActionGenerated -= actionController.AddAction;
        editorMode.OnActionGenerated -= actionController.AddAction;

    }

    private void Update()
    {
        if (!isEditModeActivated) return;

        if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
        {
            if (Utils.isCursorLocked || isAdvancedModeActive)
                CheckEditModeInput();
            if (currentActiveMode != null)
                currentActiveMode.CheckInput();
        }

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

    void CatalogReceived(string catalogJson)
    {
        AssetCatalog.i.AddFullSceneObjectCatalog(catalogJson);
        catalogAdded = true;
    }

    void StopInput()
    {
        builderInputWrapper.StopInput();
    }

    void ResumeInput()
    {
        builderInputWrapper.ResumeInput();
    }

    void InitEditModes()
    {
        firstPersonMode.Init(editionGO, undoGO, snapGO, freeMovementGO, builderInWorldEntityHandler.GetSelectedEntityList());
        editorMode.Init(editionGO, undoGO, snapGO, freeMovementGO, builderInWorldEntityHandler.GetSelectedEntityList());

        firstPersonMode.OnInputDone += InputDone;
        editorMode.OnInputDone += InputDone;

        firstPersonMode.OnActionGenerated += actionController.AddAction;
        editorMode.OnActionGenerated += actionController.AddAction;

    }

    void StartTutorial()
    {
        TutorialController.i.SetTutorialEnabled(false.ToString(),TutorialController.TutorialType.BuilderInWorld);
    }

    void MouseClick(int buttonID, Vector3 position)
    {
        if (!isEditModeActivated) return;

        if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
        {
            if (Utils.isCursorLocked || isAdvancedModeActive)
            {
                if (buttonID == 0)
                {
                    MouseClickDetected();
                    InputDone();
                    return;
                }
                CheckOutline();
            }
        }

    }

    bool IsInsideTheLimits(SceneObject sceneObject)
    {
        SceneMetricsController.Model limits = sceneToEdit.metricsController.GetLimits();
        SceneMetricsController.Model usage = sceneToEdit.metricsController.GetModel();

        if (limits.bodies < usage.bodies + sceneObject.metrics.bodies)
        {

            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.entities < usage.entities + sceneObject.metrics.entities)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.materials < usage.materials + sceneObject.metrics.materials)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.textures < usage.textures + sceneObject.metrics.textures)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        return true;
    }
    void CreateSceneObjectSelected(SceneObject sceneObject)
    {
        if (!IsInsideTheLimits(sceneObject)) return;

        //Note (Adrian): This is a workaround until the mapping is handle by kernel

        LoadParcelScenesMessage.UnityParcelScene data = sceneToEdit.sceneData;
        data.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;

        foreach (KeyValuePair<string, string> content in sceneObject.contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            bool found = false;
            foreach (ContentServerUtils.MappingPair mappingPairToCheck in data.contents)
            {
                if (mappingPairToCheck.file == mappingPair.file)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                data.contents.Add(mappingPair);
        }
        SceneController.i.UpdateParcelScenesExecute(data);

        //

        GLTFShape mesh = (GLTFShape)sceneToEdit.SharedComponentCreate(sceneObject.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
        mesh.model = new LoadableShape.Model();
        mesh.model.src = sceneObject.model;


        DecentralandEntityToEdit entity = builderInWorldEntityHandler.CreateEntity(sceneToEdit, currentActiveMode.GetCreatedEntityPoint(), editionGO.transform.position);
        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, mesh.id);

        if (sceneObject.asset_pack_id == BuilderInWorldSettings.VOXEL_ASSETS_PACK_ID)
            entity.isVoxel = true;
        builderInWorldEntityHandler.DeselectEntities();
        builderInWorldEntityHandler.Select(entity.rootEntity);


        entity.gameObject.transform.eulerAngles = Vector3.zero;

        currentActiveMode.CreatedEntity(entity);
        if (!isAdvancedModeActive)
            Utils.LockCursor();
        lastSceneObjectCreated = sceneObject;

        builderInWorldBridge.AddEntityOnKernel(entity.rootEntity,sceneToEdit); 
        InputDone();
        OnSceneObjectPlaced?.Invoke();
    }

    void CreateLastSceneObject()
    {
        if (lastSceneObjectCreated != null)
        {
            if (builderInWorldEntityHandler.IsAnyEntitySelected())
                builderInWorldEntityHandler.DeselectEntities();
            CreateSceneObjectSelected(lastSceneObjectCreated);
            InputDone();
        }
    }

    void ChangeSnapMode()
    {
        SetSnapActive(!isSnapActive);
        InputDone();
    }

    void RedoAction()
    {
        actionController.TryToRedoAction();
        InputDone();
    }

    void UndoAction()
    {
        actionController.TryToUndoAction();
        InputDone();
    }

    void CheckEditModeInput()
    {
        if (!builderInWorldEntityHandler.IsAnyEntitySelected() || isMultiSelectionActive)
        {
            CheckOutline();
        }

        if (builderInWorldEntityHandler.IsAnyEntitySelected())
        {
            currentActiveMode.CheckInputSelectedEntities();
        }
    }

    public void ChangeAdvanceMode()
    {
        SetAdvanceMode(!isAdvancedModeActive);
        InputDone();
    }

    public void SetBuildMode(EditModeState state)
    {
        if(currentActiveMode != null)
            currentActiveMode.Desactivate();
        isAdvancedModeActive = false;

        currentActiveMode = null;
        switch (state)
        {
            case EditModeState.Inactive:               
                break;
            case EditModeState.FirstPerson:
                currentActiveMode = firstPersonMode;
                HUDController.i.buildModeHud.ActivateFirstPersonModeUI();
                HUDController.i.buildModeHud.SetVisibilityOfCatalog(false);
                cursorGO.SetActive(true);
                break;
            case EditModeState.Editor:
                cursorGO.SetActive(false);
                currentActiveMode = editorMode;
                isAdvancedModeActive = true;
                HUDController.i.buildModeHud.ActivateGodModeUI();
                break;
        }
        if (currentActiveMode != null)
        {
            currentActiveMode.Activate(sceneToEdit);
            currentActiveMode.SetSnapActive(isSnapActive);
            builderInWorldEntityHandler.SetActiveMode(currentActiveMode);
        }

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

    void StartMultiSelection()
    {
        isMultiSelectionActive = true;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        currentActiveMode.StartMultiSelection();
    }

    void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        currentActiveMode.EndMultiSelection();
        outlinerController.CancelUnselectedOutlines();
    }

    private void CheckOutline()
    {
        if (outlinerOptimizationCounter >= 10 && isOutlineCheckActive)
        {
            if (!BuildModeUtils.IsPointerOverUIElement())
            {
                DecentralandEntityToEdit entity = GetEntityOnPointer();
                if (!isMultiSelectionActive)
                    outlinerController.CancelAllOutlines();
                else
                    outlinerController.CancelUnselectedOutlines();

                if (entity != null && !entity.IsSelected)             
                    outlinerController.OutlineEntity(entity);
                
            }
            outlinerOptimizationCounter = 0;
        }
        else outlinerOptimizationCounter++;
    }

    public void UndoEditionGOLastStep()
    {
        BuildModeUtils.CopyGameObjectStatus(undoGO, editionGO, false, false);
    }

    public void ResetScaleAndRotation()
    {
        currentActiveMode.ResetScaleAndRotation();
      
    }
    public void SetOutlineCheckActive(bool isActive)
    {
        isOutlineCheckActive = isActive;
    }
    public void SetSnapActive(bool isActive)
    {      
        isSnapActive = isActive;
        currentActiveMode.SetSnapActive(isActive);
    }

    void InputDone()
    {
        nexTimeToReceiveInput = Time.timeSinceLevelLoad+msBetweenInputInteraction/1000;      
    }

    private void OnEditModeChangeAction(DCLAction_Trigger action)
    {
        if (activeFeature)
        {
            if (isEditModeActivated)
            {
                ExitEditMode();
            }
            else
            {
                StartEnterEditMode();
            }
        }
    }

    void MouseClickDetected()
    {        
        DecentralandEntityToEdit entityToSelect = GetEntityOnPointer();
        if (entityToSelect != null)
        {
            builderInWorldEntityHandler.EntityClicked(entityToSelect);
        }
        else if (!isMultiSelectionActive)
        {
            builderInWorldEntityHandler.DeselectEntities();
        }
    }

    public DecentralandEntityToEdit GetEntityOnPointer()
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
        UnityEngine.Ray ray  = Camera.main.ScreenPointToRay(Input.mousePosition); ;

        float currentDistance = 9999;
        VoxelEntityHit voxelEntityHit = null;
        DecentralandEntityToEdit unselectedEntity = null;

        hits = Physics.RaycastAll(ray, RAYCAST_MAX_DISTANCE, layerToRaycast);
        foreach (RaycastHit hit in hits)
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                DecentralandEntityToEdit entityToCheck = builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);
                if (entityToCheck == null) continue;

                if (!entityToCheck.IsSelected && entityToCheck.tag == BuilderInWorldSettings.VOXEL_TAG)
                {
                    if (Vector3.Distance(Camera.main.transform.position, entityToCheck.rootEntity.gameObject.transform.position) < currentDistance)
                    {
                        unselectedEntity = entityToCheck;
                        voxelEntityHit = new VoxelEntityHit(unselectedEntity, hit);
                        currentDistance = Vector3.Distance(Camera.main.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
                    }
                }
            }
        }
        return voxelEntityHit;
    }

    public void StartEnterEditMode()
    {
        FindSceneToEdit();
        builderInWorldBridge.StartKernelEditMode(sceneToEdit);
        StartCoroutine(WaitUntilNewSceneIsLoaded());
    }

    public void EnterEditMode()
    {

        HUDController.i.buildModeHud.SetVisibility(true);
        
        isEditModeActivated = true;
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        inputController.isBuildModeActivate = true;  
   
        FindSceneToEdit();


        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);
        HUDController.i.buildModeHud.SetParcelScene(sceneToEdit);   

        if(currentActiveMode == null)
            SetBuildMode(EditModeState.Editor);

        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Stop();
        //


        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);
     
        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(true);
        builderInWorldEntityHandler.EnterEditMode(sceneToEdit);

        SceneController.i.ActiveBuilderInWorldEditScene();
     
    }

    public void ExitEditMode()
    {
        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Start();
        //

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        HUDController.i.buildModeHud.SetVisibility(false);

        inputController.isBuildModeActivate = false;
        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;
        
        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);
        builderInWorldEntityHandler.DeselectEntities();
        isEditModeActivated = false;
        sceneToEdit.SetEditMode(false);
        SetBuildMode(EditModeState.Inactive);
    
           
        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(false);
        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);

        SceneController.i.DesactiveBuilderInWorldEditScene();
    }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position)
    {
        ExitEditMode();
    }

    void FindSceneToEdit()
    {      
        foreach(ParcelScene scene in SceneController.i.scenesSortedByDistance)
        {
            if(scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
            {
                if (sceneToEdit != null && sceneToEdit != scene)
                    actionController.ClearActionList();
                sceneToEdit = scene;
                break;
            }
        }    
    }
    void PublishScene()
    {
        builderInWorldBridge.PublishScene(sceneToEdit);
    }

    IEnumerator WaitUntilNewSceneIsLoaded()
    {
        yield return new WaitForSeconds(0.8f);
        EnterEditMode();
    }
}