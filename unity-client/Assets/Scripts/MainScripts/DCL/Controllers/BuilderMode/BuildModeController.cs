using Builder;
using Builder.Gizmos;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
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
        Editor =2
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


    [Header ("Scene references")]
    public GameObject buildModeCanvasGO;
    public GameObject shortCutsGO,extraBtnsGO;
    public SceneObjectCatalogController catalogController;
    public SceneLimitInfoController sceneLimitInfoController;
    public EntityInformationController entityInformationController;
    public BuildModeEntityListController buildModeEntityListController;
    public OutlinerController outlinerController;
    public BuilderInputWrapper builderInputWrapper;
    public DCLBuilderGizmoManager gizmoManager;
    public ActionController actionController;

    [Header("Build Modes")]

    public BuildFirstPersonMode firstPersonMode;
    public BuildEditorMode editorMode;

    [Header("Build References")]

    public Texture2D duplicateCursorTexture;
    public Material editMaterial;
 
    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChange;


    BuildModeState currentActiveMode;

    ParcelScene sceneToEdit;

    bool isEditModeActivated = false, isSnapActive = true,isSceneEntitiesListActive = false, isMultiSelectionActive = false,isAdvancedModeActive = true;
    List<DecentralandEntityToEdit> selectedEntities = new List<DecentralandEntityToEdit>();
    Dictionary<string, DecentralandEntityToEdit> convertedEntities = new Dictionary<string, DecentralandEntityToEdit>();

    GameObject gameObjectToEdit;
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
        freeMovementGO.transform.SetParent(Camera.main.transform);
        if (gameObjectToEdit == null)
        {
            gameObjectToEdit = new GameObject("EditionGO");

        }
        gameObjectToEdit.transform.SetParent(Camera.main.transform);
        if (undoGO == null)
        {
            undoGO = new GameObject("UndoGameObject");
            undoGO.transform.SetParent(transform);
        }


        editModeChange.OnTriggered += OnEditModeChangeAction;
        catalogController.OnSceneObjectSelected += CreateSceneObjectSelected;
        builderInputWrapper.OnMouseClick += MouseClick;
        buildModeEntityListController.OnEntityClick += ChangeEntitySelectionFromList;
        buildModeEntityListController.OnEntityDelete += DeleteEntity;
        buildModeEntityListController.OnEntityLock += ChangeEntityLockStatus;
        buildModeEntityListController.OnEntityChangeVisibility += ChangeEntityVisibilityStatus;
        actionController.OnRedo += ReSelectEntities;
        actionController.OnUndo += ReSelectEntities;

        InitEditModes();

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

    private void OnDestroy()
    {
        editModeChange.OnTriggered -= OnEditModeChangeAction;
        DestroyCollidersForAllEntities();
    }

    private void Update()
    {        
        if(isEditModeActivated)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                StartMultiSelection();
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                EndMultiSelection();
            }

            if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
            {
                CheckInputForShowingUI();
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
        firstPersonMode.Init(gameObjectToEdit, undoGO, snapGO, freeMovementGO, selectedEntities);
        editorMode.Init(gameObjectToEdit, undoGO,  snapGO, freeMovementGO, selectedEntities);

        firstPersonMode.OnInputDone += InputDone;
        editorMode.OnInputDone += InputDone;

        firstPersonMode.OnActionGenerated += actionController.AddAction;
        editorMode.OnActionGenerated += actionController.AddAction;

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
                    CheckEntityOnPointer();
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
            if (!sceneLimitInfoController.IsActive()) ChangeVisibilityOfSceneInfo();
            return;
        }
        if (limits.entities < usage.entities + sceneObject.metrics.entities)
        {
            if (!sceneLimitInfoController.IsActive()) ChangeVisibilityOfSceneInfo();
            return;
        }
        if (limits.materials < usage.materials + sceneObject.metrics.materials)
        {
            if (!sceneLimitInfoController.IsActive()) ChangeVisibilityOfSceneInfo();
            return;
        }
        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
        {
            if (!sceneLimitInfoController.IsActive()) ChangeVisibilityOfSceneInfo();
            return;
        }
        if (limits.textures < usage.textures + sceneObject.metrics.textures)
        {
            if (!sceneLimitInfoController.IsActive()) ChangeVisibilityOfSceneInfo();
            return;
        }
        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
        {
            if (!sceneLimitInfoController.IsActive()) ChangeVisibilityOfSceneInfo();
            return;
        }

        LoadParcelScenesMessage.UnityParcelScene data =  sceneToEdit.sceneData;
        data.baseUrl = "https://builder-api.decentraland.org/v1/storage/contents/";


        foreach(KeyValuePair<string,string> content in sceneObject.contents)
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
            if(!found) data.contents.Add(mappingPair);
        }
        SceneController.i.UpdateParcelScenesExecute(data);

        GLTFShape mesh = (GLTFShape)sceneToEdit.SharedComponentCreate(sceneObject.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
        mesh.model = new LoadableShape.Model();
        mesh.model.src = sceneObject.model;


        DecentralandEntity entity = CreateEntity();
        sceneToEdit.SharedComponentAttach(entity.entityId, mesh.id);

        DeselectEntities();
        Select(entity);


        entity.gameObject.transform.eulerAngles = Vector3.zero;

        currentActiveMode.CreatedEntity(convertedEntities[GetConvertedUniqueKeyForEntity(entity)]);
        catalogController.CloseCatalog();
        lastSceneObjectCreated = sceneObject;
        InputDone();

    }


 

    void CheckInputForShowingUI()
    {
        if (Input.GetKey(KeyCode.O))
        {
            ChangeVisibilityOfUI();
            InputDone();
            return;
        }
        if (Input.GetKey(KeyCode.Y))
        {
            ChangeEntityListVisibility();
            InputDone();
            return;
        }
        if (Input.GetKey(KeyCode.J))
        {
            ChangeVisibilityOfCatalog();

            InputDone();
            return;
        }
        if (Input.GetKey(KeyCode.G))
        {
            ChangeVisibilityOfSceneInfo();
            InputDone();
            return;
        }
        if (Input.GetKey(KeyCode.L))
        {
            SetAdvanceMode(!isAdvancedModeActive);
            InputDone();
            return;
        }
            if (Input.GetKey(KeyCode.N))
        {

            ChangeVisibilityOfControls();
            InputDone();
            return;
        }

      

    }



    void CheckEditModeInput()
    {        
        if (Input.GetKeyUp(KeyCode.Q))
        {
   
            if(lastSceneObjectCreated != null)
            {
                if (selectedEntities.Count > 0) DeselectEntities();
                CreateSceneObjectSelected(lastSceneObjectCreated);
                InputDone();
            }
            //CreateBoxEntity();
        }

        if(Input.GetKeyUp(KeyCode.T))
        {
            SetSnapActive(!isSnapActive);
            InputDone();
        }

        if (selectedEntities.Count <= 0 || Input.GetKey(KeyCode.LeftShift))
        {
            CheckEntityOnPointer();
        }

        if (Input.GetKeyUp(KeyCode.I))
        {
            actionController.TryToRedoAction();
            InputDone();
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            actionController.TryToUndoAction();
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            catalogController.QuickBarObjectSelected(0);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            catalogController.QuickBarObjectSelected(1);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            catalogController.QuickBarObjectSelected(2);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            catalogController.QuickBarObjectSelected(3);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            catalogController.QuickBarObjectSelected(4);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            catalogController.QuickBarObjectSelected(5);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha7))
        {
            catalogController.QuickBarObjectSelected(6);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha8))
        {
            catalogController.QuickBarObjectSelected(7);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha9))
        {
            catalogController.QuickBarObjectSelected(8);
            InputDone();
        }


        if (selectedEntities.Count > 0)
        {
            currentActiveMode.CheckInputSelectedEntities();

            if (Input.GetKey(KeyCode.Delete))
            {
                DeletedSelectedEntities();
                InputDone();
                return;
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D))
            {
                DuplicateEntities();
                InputDone();
                return;
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Z))
            {
                CancelCurrentAction();
                DeselectEntities();
                InputDone();
                return;
            }
                 
        }
    }

    public void ChangeVisibilityOfControls()
    {
        shortCutsGO.SetActive(!shortCutsGO.gameObject.activeSelf);
    }
    public void ChangeVisibilityOfExtraBtns()
    {
        extraBtnsGO.SetActive(!extraBtnsGO.activeSelf);
    }
    public void ChangeVisibilityOfUI()
    {
        buildModeCanvasGO.SetActive(!buildModeCanvasGO.activeSelf);
    }
    public void ChangeEntityListVisibility()
    {
        if (isSceneEntitiesListActive) buildModeEntityListController.CloseList();
        else buildModeEntityListController.OpenEntityList(GetEntitiesInCurrentScene(),sceneToEdit);
        isSceneEntitiesListActive = !isSceneEntitiesListActive;
        InputDone();
    }
    public void ChangeVisibilityOfCatalog()
    {
        if (catalogController.IsCatalogOpen())
        {
            catalogController.CloseCatalog();
            if (!isAdvancedModeActive) Utils.LockCursor();
        }
        else catalogController.OpenCatalog();
        InputDone();
    }

    public void ChangeVisibilityOfSceneInfo()
    {
        if (sceneLimitInfoController.IsActive())
        {
            sceneLimitInfoController.Disable();
        }
        else
        {
            sceneLimitInfoController.Enable();

        }
        InputDone();
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
                Debug.Log("FirstPerson activated");
                currentActiveMode = firstPersonMode;             
                break;
            case EditModeState.Editor:
                Debug.Log("Editor activated");
                currentActiveMode = editorMode;
                isAdvancedModeActive = true;
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
    private void CheckEntityOnPointer()
    {
        if (outlinerOptimizationCounter >= 10)
        {
            if (!BuildModeUtils.IsPointerOverUIElement())
            {
                DecentralandEntityToEdit entity = GetEntityOnPointer();
                if (!isMultiSelectionActive) outlinerController.CancelAllOutlines();
                else outlinerController.CancelUnselectedOutlines();
                if (entity != null && !entity.IsSelected)
                {
                    outlinerController.OutLineEntity(entity);
                }
            }
            outlinerOptimizationCounter = 0;
        }
        else outlinerOptimizationCounter++;
    }

    public void CancelCurrentAction()
    {
        List<DecentralandEntityToEdit> entitiesToRemove = new List<DecentralandEntityToEdit>();
        foreach(DecentralandEntityToEdit entity in selectedEntities)
        {
            if (entity.IsSelected && entity.IsNew) entitiesToRemove.Add(entity);
        }

        BuildModeUtils.CopyGameObjectStatus(undoGO, gameObjectToEdit,false,false);

        foreach(DecentralandEntityToEdit entity in entitiesToRemove)
        {
            selectedEntities.Remove(entity);
            convertedEntities.Remove(entity.entityUniqueId);
            sceneToEdit.RemoveEntity(entity.rootEntity.entityId, true);
        }
    

    }

    void EntityListChanged()
    {
        buildModeEntityListController.SetEntityList(GetEntitiesInCurrentScene());
    }

    public void ResetScaleAndRotation()
    {
        currentActiveMode.ResetScaleAndRotation();
      
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
            if (!isMultiSelectionActive) outlinerController.OutLineEntity(entityToEdit);
            else outlinerController.OutlineEntities(selectedEntities);
        }
       
    }
    void Select(DecentralandEntity decentralandEntity)
    {
        if (convertedEntities.ContainsKey(sceneToEdit.sceneData.id + decentralandEntity.entityId))
        {
            DecentralandEntityToEdit entityEditable = convertedEntities[sceneToEdit.sceneData.id + decentralandEntity.entityId];
            SelectEntity(entityEditable);
        }
    }
    bool SelectEntity(DecentralandEntityToEdit entityEditable)
    {
        
        if (entityEditable.IsLocked) return false;

        if (entityEditable.IsSelected) return false;
       
        entityEditable.Select();

        selectedEntities.Add(entityEditable);


        currentActiveMode.SelectedEntity(entityEditable);


        entityInformationController.Enable();
        entityInformationController.SetEntity(entityEditable.rootEntity, sceneToEdit);

    

        sceneLimitInfoController.UpdateInfo();
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

    DecentralandEntityToEdit GetEntityOnPointer()
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
    void DeselectEntity(DecentralandEntityToEdit entity)
    {
        if (!selectedEntities.Contains(entity)) return;

        if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.rootEntity))
        {
            CancelCurrentAction();
        }
 
        SceneController.i.boundariesChecker.EvaluateEntityPosition(entity.rootEntity);
        SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entity.rootEntity);
        entity.Deselect();
        outlinerController.CancelEntityOutline(entity);
        selectedEntities.Remove(entity);
        currentActiveMode.EntityDeselected(entity);
        if (selectedEntities.Count <= 0) entityInformationController.Disable();
    }
    void DeselectEntities()
    {
        if (selectedEntities.Count > 0)
        {
            if (!AreAllSelectedEntitiesInsideBoundaries()) CancelCurrentAction();

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

    public void DeleteEntity(DecentralandEntityToEdit entityToDelete)
    {
        if (entityToDelete.IsSelected) DeselectEntity(entityToDelete);
        RemoveConvertedEntity(entityToDelete.rootEntity);
        entityToDelete.Delete();
        sceneToEdit.RemoveEntity(entityToDelete.rootEntity.entityId, true);
        sceneLimitInfoController.UpdateInfo();
        EntityListChanged();
    }


    public void DuplicateEntity(DecentralandEntityToEdit entityToDuplicate)
    {
        DecentralandEntity entity = sceneToEdit.DuplicateEntity(entityToDuplicate.rootEntity);

        BuildModeUtils.CopyGameObjectStatus(entityToDuplicate.gameObject, entity.gameObject, false, false);
        SetupEntityToEdit(entity);
        sceneLimitInfoController.UpdateInfo();

        currentActiveMode.SetDuplicationOffset(entityToDuplicate,duplicateOffset);
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
        Cursor.SetCursor(duplicateCursorTexture, Vector2.zero, CursorMode.Auto);
        EntityListChanged();

    }

    DecentralandEntity CreateEntity()
    {

        DecentralandEntity newEntity = sceneToEdit.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(currentActiveMode.GetCreatedEntityPoint(), sceneToEdit);

        Vector3 pointToLookAt = Camera.main.transform.position;
        pointToLookAt.y = gameObjectToEdit.transform.position.y;
        Quaternion lookOnLook = Quaternion.LookRotation(gameObjectToEdit.transform.position - pointToLookAt);

        DCLTransform.model.rotation = lookOnLook;
        DCLTransform.model.scale = newEntity.gameObject.transform.lossyScale;

        sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);      


      
        SetupEntityToEdit(newEntity,true);
        sceneLimitInfoController.UpdateInfo();
        EntityListChanged();
        return newEntity;
    }
    void CreateBoxEntity()
    {

        DecentralandEntity newEntity = CreateEntity();

        BaseDisposable mesh = sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.BOX_SHAPE));
        sceneToEdit.SharedComponentAttach(newEntity.entityId, mesh.id);

        BaseDisposable material = sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.PBR_MATERIAL));

        ((PBRMaterial)material).model.albedoColor = editMaterial.color;
        sceneToEdit.SharedComponentAttach(newEntity.entityId, material.id);

        if(isSnapActive) newEntity.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        Select(newEntity);

        sceneLimitInfoController.UpdateInfo();
    }


    public void EnterEditMode()
    {
        
        buildModeCanvasGO.SetActive(true);
        DCLCharacterController.i.SetFreeMovementActive(true);
        isEditModeActivated = true;
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

   
        FindSceneToEdit();
        sceneToEdit.SetEditMode(true);
        sceneLimitInfoController.SetParcelScene(sceneToEdit);
    

        if(currentActiveMode == null) SetBuildMode(EditModeState.Editor);

        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Stop();
        //
        CommonScriptableObjects.allUIHidden.Set(true);

        SetupAllEntities();
        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(true);
    }


    public void ExitEditMode()
    {
        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Start();
        //

        CommonScriptableObjects.allUIHidden.Set(false);
        buildModeCanvasGO.SetActive(false);

        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;
        DCLCharacterController.i.SetFreeMovementActive(false);
        outlinerController.CancelAllOutlines();
        
        DeselectEntities();
        isEditModeActivated = false;
        sceneToEdit.SetEditMode(false);
        SetBuildMode(EditModeState.Inactive);

        if (isSceneEntitiesListActive) ChangeEntityListVisibility();
    
           
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

    string GetConvertedUniqueKeyForEntity(DecentralandEntity entity)
    {
        return entity.scene.sceneData.id + entity.entityId;
    }


    
}