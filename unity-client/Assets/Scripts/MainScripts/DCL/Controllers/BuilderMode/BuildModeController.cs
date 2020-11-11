using Builder;
using Builder.Gizmos;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
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

public class BuildModeController : ViewController
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
    public float duplicateOffset = 2f;



    [Header("Snap variables")]
    public float snapFactor = 1f;
    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;

    public float snapDistanceToActivateMovement = 10f;


    [Header("Scene references")]
    public GameObject cameraParentGO,cursorGO;

    public EntityInformationController entityInformationController;
    public OutlinerController outlinerController;
    public BuilderInputWrapper builderInputWrapper;
    public DCLBuilderGizmoManager gizmoManager;
    public ActionController actionController;
    public InputController inputController;

    [Header("Build Modes")]

    public BuildFirstPersonMode firstPersonMode;
    public BuildEditorMode editorMode;

    [Header("Build References")]

    public Texture2D duplicateCursorTexture;
    public Material editMaterial;

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

    BuildModeState currentActiveMode;

    ParcelScene sceneToEdit;

    bool isEditModeActivated = false, isSnapActive = true, isMultiSelectionActive = false, isAdvancedModeActive = true, isOutlineCheckActive = true;
    List<DecentralandEntityToEdit> selectedEntities = new List<DecentralandEntityToEdit>();
    Dictionary<string, DecentralandEntityToEdit> convertedEntities = new Dictionary<string, DecentralandEntityToEdit>();

    GameObject editionGO;
    GameObject undoGO, snapGO, freeMovementGO;

    float nexTimeToReceiveInput;

    int outlinerOptimizationCounter = 0, checkerInsideSceneOptimizationCounter = 0;

    SceneObject lastSceneObjectCreated;

    void Start()
    {
        if (snapGO == null)
        {
            snapGO = new GameObject("SnapGameObject");

        }
        snapGO.transform.SetParent(transform);
        if (freeMovementGO == null)
        {
            freeMovementGO = new GameObject("FreeMovementGO");

        }
        freeMovementGO.transform.SetParent(cameraParentGO.transform);
        if (editionGO == null)
        {
            editionGO = new GameObject("EditionGO");

        }
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

        toggleCreateLastSceneObjectInputAction.OnTriggered += (o) => CreateLastSceneObject();
        toggleRedoActionInputAction.OnTriggered += (o) => RedoAction();
        toggleUndoActionInputAction.OnTriggered += (o) => UndoAction();
        toggleSnapModeInputAction.OnTriggered += (o) => ChangeSnapMode();

        HUDController.i.buildModeHud.OnChangeModeAction += ChangeAdvanceMode;
        HUDController.i.buildModeHud.OnDuplicateSelectedAction += DuplicateSelectedEntitiesInput;
        HUDController.i.buildModeHud.OnDeleteSelectedAction += DeleteSelectedEntitiesInput;
        HUDController.i.buildModeHud.OnResetAction += ResetScaleAndRotation;

        multiSelectionInputAction.OnStarted += (o) => StartMultiSelection();
        multiSelectionInputAction.OnFinished += (o) => EndMultiSelection();

        HUDController.i.buildModeHud.OnEntityClick += ChangeEntitySelectionFromList;
        HUDController.i.buildModeHud.OnEntityDelete += DeleteEntity;
        HUDController.i.buildModeHud.OnEntityLock += ChangeEntityLockStatus;
        HUDController.i.buildModeHud.OnEntityChangeVisibility += ChangeEntityVisibilityStatus;

        HUDController.i.buildModeHud.OnStopInput += () => builderInputWrapper.StopInput();
        HUDController.i.buildModeHud.OnResumeInput += () => builderInputWrapper.ResumeInput();
        HUDController.i.buildModeHud.OnSceneObjectSelected += CreateSceneObjectSelected;
        HUDController.i.buildModeHud.OnTutorialAction += StartTutorial;
        builderInputWrapper.OnMouseClick += MouseClick;

        actionController.OnRedo += ReSelectEntities;
        actionController.OnUndo += ReSelectEntities;

        InitEditModes();

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);
    }


    private void OnDestroy()
    {
        editModeChangeInputAction.OnTriggered -= OnEditModeChangeAction;
        DestroyCollidersForAllEntities();
    }

    private void Update()
    {
        if (isEditModeActivated)
        {
            if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
            {
                if (Utils.isCursorLocked || isAdvancedModeActive) CheckEditModeInput();
                if (currentActiveMode != null) currentActiveMode.CheckInput();
            }

            if (checkerInsideSceneOptimizationCounter >= 60)
            {
                if (!sceneToEdit.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition)) ExitEditMode();
                checkerInsideSceneOptimizationCounter = 0;
            }
            else checkerInsideSceneOptimizationCounter++;
        }
    }

    void InitEditModes()
    {
        firstPersonMode.Init(editionGO, undoGO, snapGO, freeMovementGO, selectedEntities);
        editorMode.Init(editionGO, undoGO, snapGO, freeMovementGO, selectedEntities);

        firstPersonMode.OnInputDone += InputDone;
        editorMode.OnInputDone += InputDone;

        firstPersonMode.OnActionGenerated += actionController.AddAction;
        editorMode.OnActionGenerated += actionController.AddAction;

    }

    void StartTutorial()
    {
        TutorialController.i.SetTutorialEnabled(false.ToString(),TutorialController.TutorialType.BuilderInWorld);
    }
    private void ChangeEntityVisibilityStatus(DecentralandEntityToEdit entityToApply)
    {
        entityToApply.ChangeShowStatus();
        if (!entityToApply.IsVisible && selectedEntities.Contains(entityToApply)) DeselectEntity(entityToApply);
    }

    private void ChangeEntityLockStatus(DecentralandEntityToEdit entityToApply)
    {
        entityToApply.ChangeLockStatus();
        if (entityToApply.IsLocked && selectedEntities.Contains(entityToApply)) DeselectEntity(entityToApply);
    }
    void MouseClick(int buttonID, Vector3 position)
    {
        if (isEditModeActivated)
        {
            if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
            {
                if (Utils.isCursorLocked || isAdvancedModeActive)
                {
                    if (buttonID == 0)
                    {
                        ClickDetected();
                        InputDone();
                        return;
                    }
                    CheckOutline();
                }
            }
        }
    }
    void CreateSceneObjectSelected(SceneObject sceneObject)
    {
        SceneMetricsController.Model limits = sceneToEdit.metricsController.GetLimits();
        SceneMetricsController.Model usage = sceneToEdit.metricsController.GetModel();

        if (limits.bodies < usage.bodies + sceneObject.metrics.bodies)
        {

            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return;
        }
        if (limits.entities < usage.entities + sceneObject.metrics.entities)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return;
        }
        if (limits.materials < usage.materials + sceneObject.metrics.materials)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return;
        }
        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return;
        }
        if (limits.textures < usage.textures + sceneObject.metrics.textures)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return;
        }
        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return;
        }

        LoadParcelScenesMessage.UnityParcelScene data = sceneToEdit.sceneData;
        data.baseUrl = BuilderConstants.BASE_URL_CATALOG;


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
            if (!found) data.contents.Add(mappingPair);
        }
        SceneController.i.UpdateParcelScenesExecute(data);

        GLTFShape mesh = (GLTFShape)sceneToEdit.SharedComponentCreate(sceneObject.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
        mesh.model = new LoadableShape.Model();
        mesh.model.src = sceneObject.model;


        DecentralandEntity entity = CreateEntity();
        sceneToEdit.SharedComponentAttach(entity.entityId, mesh.id);

        if (sceneObject.asset_pack_id == "b51e5e7c-c56b-4ad9-b9d2-1dc1c6546169") convertedEntities[GetConvertedUniqueKeyForEntity(entity)].IsVoxel = true;
        DeselectEntities();
        Select(entity);


        entity.gameObject.transform.eulerAngles = Vector3.zero;

        currentActiveMode.CreatedEntity(convertedEntities[GetConvertedUniqueKeyForEntity(entity)]);
        if (!isAdvancedModeActive) Utils.LockCursor();
        lastSceneObjectCreated = sceneObject;

        InputDone();
        OnSceneObjectPlaced?.Invoke();
    }

    void CreateLastSceneObject()
    {
        if (lastSceneObjectCreated != null)
        {
            if (selectedEntities.Count > 0) DeselectEntities();
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


    void DeleteSelectedEntitiesInput()
    {
        if (selectedEntities.Count > 0)
        {
            DeletedSelectedEntities();
            InputDone();
        }

    }

    void DuplicateSelectedEntitiesInput()
    {
        if (selectedEntities.Count > 0)
        {
            DuplicateEntities();
            InputDone();
        }
    }
    void CheckEditModeInput()
    {

        if (selectedEntities.Count <= 0 || isMultiSelectionActive)
        {
            CheckOutline();
        }


        if (selectedEntities.Count > 0)
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
        if(currentActiveMode != null)currentActiveMode.Desactivate();
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
        }
        DeselectEntities();
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
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        currentActiveMode.StartMultiSelection();

    }

    void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        currentActiveMode.EndMultiSelection();
        outlinerController.CancelUnselectedOutlines();
    }

    bool AreAllSelectedEntitiesInsideBoundaries()
    {
        bool areAllIn = true;
        foreach(DecentralandEntityToEdit entity in selectedEntities)
        {
            if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
            {
                areAllIn = false;
                break;
            }
        }
        return areAllIn;
    }
    private void CheckOutline()
    {
        if (outlinerOptimizationCounter >= 10 && isOutlineCheckActive)
        {
            if (!BuildModeUtils.IsPointerOverUIElement())
            {
                DecentralandEntityToEdit entity = GetEntityOnPointer();
                if (!isMultiSelectionActive) outlinerController.CancelAllOutlines();
                else outlinerController.CancelUnselectedOutlines();
                if (entity != null && !entity.IsSelected)
                {
                    outlinerController.OutlineEntity(entity);
                }
            }
            outlinerOptimizationCounter = 0;
        }
        else outlinerOptimizationCounter++;
    }

    public void DestroyCreatedObjects()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();
        foreach(DecentralandEntityToEdit entity in selectedEntities)
        {
            if (entity.IsSelected && entity.IsNew) entitiesToRemove.Add(entity);
        }

        BuildModeUtils.CopyGameObjectStatus(undoGO, editionGO,false,false);

        foreach(DecentralandEntityToEdit entity in entitiesToRemove)
        {
            DeleteEntity(entity,false);
        }
    

    }

    void EntityListChanged()
    {
        HUDController.i.buildModeHud.SetEntityList(GetEntitiesInCurrentScene());
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
                EnterEditMode();
            }
        }
    }
    void EntityClicked(DecentralandEntityToEdit entityCliked)
    {
        if (entityCliked.IsSelected) DeselectEntity(entityCliked);
        else SelectEntity(entityCliked);
    }

    void ChangeEntitySelectionFromList(DecentralandEntityToEdit entityToEdit)
    {
        if (!selectedEntities.Contains(entityToEdit)) SelectFromList(entityToEdit);
        else DeselectEntity(entityToEdit);
    }
    void SelectFromList(DecentralandEntityToEdit entityToEdit)
    {
        if(!isMultiSelectionActive)DeselectEntities();
        if (SelectEntity(entityToEdit))
        {
            if (!isMultiSelectionActive) outlinerController.OutlineEntity(entityToEdit);
            else outlinerController.OutlineEntities(selectedEntities);
        }
       
    }
    public void Select(DecentralandEntity decentralandEntity)
    {
        if (convertedEntities.ContainsKey(sceneToEdit.sceneData.id + decentralandEntity.entityId))
        {
            DecentralandEntityToEdit entityEditable = convertedEntities[sceneToEdit.sceneData.id + decentralandEntity.entityId];
            SelectEntity(entityEditable);
        }
    }
   public  bool SelectEntity(DecentralandEntityToEdit entityEditable)
    {
        
        if (entityEditable.IsLocked) return false;

        if (entityEditable.IsSelected) return false;
       
        entityEditable.Select();

        selectedEntities.Add(entityEditable);


        currentActiveMode.SelectedEntity(entityEditable);


        if (entityInformationController != null)
        {
            entityInformationController.Enable();
            entityInformationController.SetEntity(entityEditable.rootEntity, sceneToEdit);
        }


        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        outlinerController.CancelAllOutlines();
        return true;
    }

    void ClickDetected()
    {        
        DecentralandEntityToEdit entityToSelect = GetEntityOnPointer();
        if (entityToSelect != null)
        {
            if (selectedEntities.Count <= 0) EntityClicked(entityToSelect);
            else
            {
                if (!isMultiSelectionActive)
                {
                    DeselectEntities();
                }
                else
                {
                    EntityClicked(entityToSelect);
                }
            }
        }
        else if (!isMultiSelectionActive) DeselectEntities();
    }

    public DecentralandEntityToEdit GetEntityOnPointer()
    {
        RaycastHit hit;
        UnityEngine.Ray ray;
        float distanceToSelect = distanceLimitToSelectObjects;
        if (!isAdvancedModeActive) ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
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
                if(convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(sceneToEdit.entities[entityID])))
                {
                    return convertedEntities[GetConvertedUniqueKeyForEntity(sceneToEdit.entities[entityID])];
                }
              
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

        hits = Physics.RaycastAll(ray, 9999, layerToRaycast);
        foreach (RaycastHit hit in hits)
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                if (convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(sceneToEdit.entities[entityID])))
                {
                    DecentralandEntityToEdit entityToCheck  = convertedEntities[GetConvertedUniqueKeyForEntity(sceneToEdit.entities[entityID])];
                    if(!entityToCheck.IsSelected && entityToCheck.tag == "Voxel")
                    {
                        if (Vector3.Distance(Camera.main.transform.position, entityToCheck.rootEntity.gameObject.transform.position) < currentDistance)
                        {
                            unselectedEntity = entityToCheck;
                            voxelEntityHit = new VoxelEntityHit(unselectedEntity,hit);
                            currentDistance = Vector3.Distance(Camera.main.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
                        }
                    }
                }

            }
        }


        return voxelEntityHit;
    }
    public List<DecentralandEntityToEdit> GetAllEntitiesFromCurrentScene()
    {
        List<DecentralandEntityToEdit> entities = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit) entities.Add(entity);
        }

        return entities;
    }
    public List<DecentralandEntityToEdit> GetAllVoxelsEntities()
    {
        List<DecentralandEntityToEdit> voxelEntities = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit && entity.IsVoxel) voxelEntities.Add(entity);
        }

        return voxelEntities;
    }
    void ReSelectEntities()
    {
        List<DecentralandEntityToEdit> entitiesToReselect = new List<DecentralandEntityToEdit>();
        foreach(DecentralandEntityToEdit entity in selectedEntities)
        {
            entitiesToReselect.Add(entity);
        }
        DeselectEntities();

        foreach (DecentralandEntityToEdit entity in entitiesToReselect)
        {
            SelectEntity(entity);
        }
    }
    public void DeselectEntity(DecentralandEntityToEdit entity)
    {
        if (!selectedEntities.Contains(entity)) return;

        if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
        {
            DestroyCreatedObjects();
        }
 
        SceneController.i.boundariesChecker.EvaluateEntityPosition(entity.rootEntity);
        SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entity.rootEntity);
        entity.Deselect();
        outlinerController.CancelEntityOutline(entity);
        selectedEntities.Remove(entity);
        currentActiveMode.EntityDeselected(entity);
        if (selectedEntities.Count <= 0 && entityInformationController != null) entityInformationController.Disable();
    }
    public void DeselectEntities()
    {
        if (selectedEntities.Count > 0)
        {
            if (!AreAllSelectedEntitiesInsideBoundaries()) DestroyCreatedObjects();

            int amountToDeselect = selectedEntities.Count;
            for(int i = 0; i < amountToDeselect; i++)
            {
                DeselectEntity(selectedEntities[0]);
            }

            currentActiveMode.DeselectedEntities();

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
   
    }

    public void ChangeLockStateSelectedEntities()
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            entity.ChangeLockStatus();
        }
        DeselectEntities();
    }

    public void ChangeShowStateSelectedEntities()
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            entity.ChangeShowStatus();
        }
    }

    public void DeletedSelectedEntities()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();

        for (int i = 0; i < selectedEntities.Count; i++)
        {         
            entitiesToRemove.Add(selectedEntities[i]);          
        }

        DeselectEntities();

        foreach(DecentralandEntityToEdit entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }

    }
    public void DeleteEntitiesOutsideSceneBoundaries()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();
        foreach (DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit)
            {
                if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
                {
                    entitiesToRemove.Add(entity);
                }
            }
        }

        foreach (DecentralandEntityToEdit entity in entitiesToRemove)
        {
            DeleteEntity(entity);
        }

    }

    public void DeleteEntity(string entityId)
    {
        DecentralandEntityToEdit entity = convertedEntities[GetConvertedUniqueKeyForEntity(entityId)];
        DeleteEntity(entity, true);
    }
    public void DeleteEntity(DecentralandEntityToEdit entityToDelete)
    {
        DeleteEntity(entityToDelete, true);
    }
    public void DeleteEntity(DecentralandEntityToEdit entityToDelete, bool checkSelection = true)
    {
        if (entityToDelete.IsSelected && checkSelection) DeselectEntity(entityToDelete);
        RemoveConvertedEntity(entityToDelete.rootEntity);
        entityToDelete.Delete();
        string idToRemove = entityToDelete.rootEntity.entityId;
        Destroy(entityToDelete);
        sceneToEdit.RemoveEntity(idToRemove, true);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        EntityListChanged();
    }


    public DecentralandEntity DuplicateEntity(DecentralandEntityToEdit entityToDuplicate)
    {
        DecentralandEntity entity = sceneToEdit.DuplicateEntity(entityToDuplicate.rootEntity);

        BuildModeUtils.CopyGameObjectStatus(entityToDuplicate.gameObject, entity.gameObject, false, false);
        SetupEntityToEdit(entity);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();


        return entity;
    }
    public void DuplicateEntities()
    {
        foreach(DecentralandEntityToEdit entity in selectedEntities)
        {
            if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity)) return;
        }

        int amount = selectedEntities.Count;
        for (int i = 0; i < amount; i++)
        {
            DuplicateEntity(selectedEntities[i]); 
        }
        currentActiveMode.SetDuplicationOffset(duplicateOffset);
        Cursor.SetCursor(duplicateCursorTexture, Vector2.zero, CursorMode.Auto);
        EntityListChanged();

    }

    public DecentralandEntity CreateEntityFromJSON(string entityJson)
    {
        DecentralandEntity newEntity = JsonConvert.DeserializeObject<DecentralandEntity>(entityJson);
        sceneToEdit.CreateEntity(newEntity.entityId);

        SetupEntityToEdit(newEntity, true);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        EntityListChanged();
        return newEntity;
    }
    DecentralandEntity CreateEntity()
    {

        DecentralandEntity newEntity = sceneToEdit.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(currentActiveMode.GetCreatedEntityPoint(), sceneToEdit);

        Vector3 pointToLookAt = Camera.main.transform.position;
        pointToLookAt.y = editionGO.transform.position.y;
        Quaternion lookOnLook = Quaternion.LookRotation(editionGO.transform.position - pointToLookAt);

        DCLTransform.model.rotation = lookOnLook;
        DCLTransform.model.scale = newEntity.gameObject.transform.lossyScale;

        sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);      


      
        SetupEntityToEdit(newEntity,true);
        HUDController.i.buildModeHud.UpdateSceneLimitInfo();
        EntityListChanged();
        return newEntity;
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
      
    

        if(currentActiveMode == null) SetBuildMode(EditModeState.Editor);

        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Stop();
        //


        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);
     
        SetupAllEntities();
        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(true);
        EntityListChanged();
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
        DeselectEntities();
        isEditModeActivated = false;
        sceneToEdit.SetEditMode(false);
        SetBuildMode(EditModeState.Inactive);

    
           
        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(false);
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
                if (sceneToEdit != null && sceneToEdit != scene) actionController.ClearActionList();
                sceneToEdit = scene;
                break;
            }
        }
     
    }

    void DestroyCollidersForAllEntities()
    {
        foreach(DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            entity.DestroyColliders();
        }
    }

    void SetupAllEntities()
    {       
        foreach (DecentralandEntity entity in sceneToEdit.entities.Values)
        {
            SetupEntityToEdit(entity);
        }
    }

    void SetupEntityToEdit(DecentralandEntity entity,bool hasBeenCreated = false)
    {
        if (!convertedEntities.ContainsKey(GetConvertedUniqueKeyForEntity(entity)))
        {           
            DecentralandEntityToEdit entityToEdit = Utils.GetOrCreateComponent<DecentralandEntityToEdit>(entity.gameObject);
            entityToEdit.Init(entity, editMaterial);
            convertedEntities.Add(entityToEdit.entityUniqueId, entityToEdit);
            entity.OnRemoved += RemoveConvertedEntity;
            entityToEdit.IsNew = hasBeenCreated;
        }

    }

    List<DecentralandEntityToEdit> GetEntitiesInCurrentScene()
    {
        List<DecentralandEntityToEdit> currentEntitiesInScene = new List<DecentralandEntityToEdit>();
        foreach(DecentralandEntityToEdit entity in convertedEntities.Values)
        {
            if (entity.rootEntity.scene == sceneToEdit) currentEntitiesInScene.Add(entity);
        }
        return currentEntitiesInScene;
    }

    void RemoveConvertedEntity(DecentralandEntity entity)
    {
        convertedEntities.Remove(GetConvertedUniqueKeyForEntity(entity));
    }

    string GetConvertedUniqueKeyForEntity(string entityID)
    {
        return sceneToEdit + entityID;
    }
    string GetConvertedUniqueKeyForEntity(DecentralandEntity entity)
    {
        return entity.scene.sceneData.id + entity.entityId;
    }


    
}