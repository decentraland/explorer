using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BuildModeController : MonoBehaviour
{
    public enum EditModeState
    {
        Inactive = 0,
        Active = 1,
        Selected_Object =2
    }

    [Header("Activation of Feature")]
    public bool activeFeature = false;
    [Header("Design variables")]

    public float scaleSpeed = 0.25f;
    public float rotationSpeed = 0.5f;
    public float msBetweenInputInteraction = 200;
    public float distanceFromCameraForNewEntitties = 4;

    public float distanceLimitToSelectObjects = 50;

 

    [Header("Snap variables")]
    public float snapFactor = 1f;
    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;

    public float snapDistanceToActivateMovement = 10f;


    [Header ("Scene references")]
    public GameObject editModeChangeFX;
    public GameObject snapImgStatusShowGO, shortCutsGO;
    public SceneObjectCatalogController catalogController;
    public SceneLimitInfoController sceneLimitInfoController;
    public EntityInformationController entityInformationController;
    public BuildModeEntityListController buildModeEntityListController;
    public OutlinerController outlinerController;

    [Header("Build References")]

    public Material editMaterial;
 
    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChange;


    ParcelScene sceneToEdit;

    bool isEditModeActivated = false, isSnapActivated = true, isSceneInformationActive = false,isSceneEntitiesListActive = false, isMultiSelectionActive = false;

    List<DecentrelandEntityToEdit> selectedEntities = new List<DecentrelandEntityToEdit>();
    Dictionary<string, DecentrelandEntityToEdit> convertedEntities = new Dictionary<string, DecentrelandEntityToEdit>();

    GameObject gameObjectToEdit;
    GameObject undoGO, snapGO, freeMovementGO;

    Quaternion initialRotation;

    Transform originalParentGOEdit;

    float currentScaleAdded, currentYRotationAdded, nexTimeToReceiveInput;

    int outlinerOptimizationCounter = 0, checkerInsideSceneOptimizationCounter = 0;

    bool snapObjectAlreadyMoved = false;

    // Start is called before the first frame update
    void Start()
    {
        editModeChange.OnTriggered += OnEditModeChangeAction;
        catalogController.OnSceneObjectSelected += OnSceneObjectSelected;

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
                CheckInputForShowingWindows();
                if (Utils.isCursorLocked) CheckEditModeInput();              
            }

            if (checkerInsideSceneOptimizationCounter >= 60)
            {
                if (!sceneToEdit.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition)) ExitEditMode();
                checkerInsideSceneOptimizationCounter = 0;
            }
            else checkerInsideSceneOptimizationCounter++;
        }
    }

    void LateUpdate()
    {
        if (selectedEntities.Count > 0 && !isMultiSelectionActive)
        {         
            if (isSnapActivated)
            {
                if (snapObjectAlreadyMoved)
                {
                    Vector3 objectPosition = snapGO.transform.position;
                    Vector3 eulerRotation = snapGO.transform.rotation.eulerAngles;

                    float currentSnapFactor = snapFactor;

                    //float currentSnapFactor = snapFactor * currentScaleAdded;

                    objectPosition.x = Mathf.RoundToInt(objectPosition.x / currentSnapFactor) * currentSnapFactor;
                    objectPosition.y = Mathf.RoundToInt(objectPosition.y / currentSnapFactor) * currentSnapFactor;
                    objectPosition.z = Mathf.RoundToInt(objectPosition.z / currentSnapFactor) * currentSnapFactor;
                    eulerRotation.y = snapRotationDegresFactor * Mathf.FloorToInt((eulerRotation.y % snapRotationDegresFactor));

                    Quaternion destinationRotation = Quaternion.AngleAxis(currentYRotationAdded, Vector3.up);
                    gameObjectToEdit.transform.rotation = initialRotation * destinationRotation;
                    gameObjectToEdit.transform.position = objectPosition;
                }
                else if (Vector3.Distance(snapGO.transform.position, gameObjectToEdit.transform.position) >= snapDistanceToActivateMovement)
                {
                    CopyGameObjectStatus(gameObjectToEdit, snapGO, false);
                    gameObjectToEdit.transform.SetParent(Camera.main.transform);
                    
                    snapObjectAlreadyMoved = true;
                }

            }
            else
            {
                Vector3 pointToLookAt = Camera.main.transform.position;
                pointToLookAt.y = gameObjectToEdit.transform.position.y;
                Quaternion lookOnLook = Quaternion.LookRotation(gameObjectToEdit.transform.position - pointToLookAt);
                freeMovementGO.transform.rotation = lookOnLook;
            }
        }
    }


    void OnSceneObjectSelected(SceneObject sceneObject)
    {
        SceneMetricsController.Model limits = sceneToEdit.metricsController.GetLimits();
        SceneMetricsController.Model usage = sceneToEdit.metricsController.GetModel();

        if (limits.bodies < usage.bodies + sceneObject.metrics.bodies) return;
        if (limits.entities < usage.entities + sceneObject.metrics.entities) return;
        if (limits.materials < usage.materials + sceneObject.metrics.materials) return;
        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes) return;
        if (limits.textures < usage.textures + sceneObject.metrics.textures) return;
        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles) return;

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


        Select(entity);
        
        catalogController.CloseCatalog();
   

    }


    void CheckInputForShowingWindows()
    {
        if (Input.GetKey(KeyCode.Y))
        {
            if (isSceneEntitiesListActive) buildModeEntityListController.CloseList();
            else buildModeEntityListController.OpenEntityList(sceneToEdit);
            isSceneEntitiesListActive = !isSceneEntitiesListActive;
            InputDone();
            return;
        }
        if (Input.GetKey(KeyCode.J))
        {
            if (catalogController.IsCatalogOpen()) catalogController.CloseCatalog();
            else catalogController.OpenCatalog();

            InputDone();
            return;
        }
        if (Input.GetKey(KeyCode.G))
        {
            if (isSceneInformationActive)
            {
                sceneLimitInfoController.Disable();
            }
            else
            {
                sceneLimitInfoController.Enable();

            }
            isSceneInformationActive = !isSceneInformationActive;
            InputDone();
            return;
        }
        if(Input.GetKey(KeyCode.N))
        {

            shortCutsGO.SetActive(!shortCutsGO.gameObject.activeSelf);
            InputDone();
            return;
        }

    }

    void CheckEditModeInput()
    {        
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (selectedEntities.Count > 0) DeselectEntities();
            CreateBoxEntity();
        }

        if(Input.GetKeyUp(KeyCode.T))
        {
            SetSnapActive(!isSnapActivated);
        }

      


        if (selectedEntities.Count <= 0 || Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectObject();
                InputDone();
                return;
            }
            CheckEntityOnPointer();
        }
        if (selectedEntities.Count > 0)
        {
            if(Input.GetKey(KeyCode.Delete))
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
                UndoEdit();
                DeselectEntities();
                InputDone();
                return;
            }
            if (Input.GetMouseButtonDown(0) && AreAllEntitiesInsideBoundaries())
            {
                DeselectEntities();
                InputDone();
                return;
            }

            if (Input.GetKey(KeyCode.R))
            {
                if (isSnapActivated)
                {

                    RotateSelection(snapRotationDegresFactor);
                    InputDone();
                }
                else
                {
                    RotateSelection(rotationSpeed);
                }
            }


            if (Input.mouseScrollDelta.y >0.5f)
            {

                if (isSnapActivated)
                {
                    ScaleSelection(snapScaleFactor);
                    InputDone();
                }
                else ScaleSelection(scaleSpeed);
            }
            else if (Input.mouseScrollDelta.y < -0.5f)
            {
                if (isSnapActivated)
                {
                    ScaleSelection(-snapScaleFactor);
                    InputDone();
                }
                else ScaleSelection(-scaleSpeed); 
            }

           
        }
    }


    void StartMultiSelection()
    {
        isMultiSelectionActive = true;
        originalParentGOEdit = gameObjectToEdit.transform.parent;

        gameObjectToEdit.transform.SetParent(null);
        snapGO.transform.SetParent(null);
        freeMovementGO.transform.SetParent(null);
    }

    void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        gameObjectToEdit.transform.SetParent(originalParentGOEdit,true);
        snapGO.transform.SetParent(Camera.main.transform);
        freeMovementGO.transform.SetParent(Camera.main.transform);

        SetObjectIfSnapOrNot();
    }

    bool AreAllEntitiesInsideBoundaries()
    {
        bool areAllIn = true;
        foreach(DecentrelandEntityToEdit entity in selectedEntities)
        {
            if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.entity))
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
            DecentralandEntity entity = GetEntityOnPointer();
            if (entity != null)
            {
                outlinerController.OutLineOnlyThisEntity(entity);
            }
            else outlinerController.CancelAllOutlines();
            outlinerOptimizationCounter = 0;
        }
        else outlinerOptimizationCounter++;
    }

    public void UndoEdit()
    {
        List<DecentrelandEntityToEdit> entitiesToRemove = new List<DecentrelandEntityToEdit>();
        foreach(DecentrelandEntityToEdit entity in selectedEntities)
        {
            if (entity.isSelected && entity.isNew) entitiesToRemove.Add(entity);
        }

        CopyGameObjectStatus(undoGO, gameObjectToEdit,false,false);

        foreach(DecentrelandEntityToEdit entity in entitiesToRemove)
        {
            selectedEntities.Remove(entity);
            convertedEntities.Remove(entity.entityUniqueId);
            sceneToEdit.RemoveEntity(entity.entity.entityId, true);
        }
    

    }

    public void ResetScaleAndRotation()
    {
        gameObjectToEdit.transform.localScale = Vector3.one;
        snapGO.transform.localScale = Vector3.one;
        freeMovementGO.transform.localScale = Vector3.one;

        currentScaleAdded = 0;
        currentYRotationAdded = 0;

        Quaternion zeroAnglesQuaternion = Quaternion.Euler(Vector3.zero);
        initialRotation = zeroAnglesQuaternion;

        snapGO.transform.rotation = zeroAnglesQuaternion;
        freeMovementGO.transform.rotation = zeroAnglesQuaternion;
        gameObjectToEdit.transform.rotation = zeroAnglesQuaternion;
      
    }
    public void SetSnapActive(bool isActive)
    {
        isSnapActivated = isActive;
        snapImgStatusShowGO.SetActive(isSnapActivated);
        if (isSnapActivated)
        {
            snapObjectAlreadyMoved = false;
            snapGO.transform.SetParent(Camera.main.transform);
        }
        SetObjectIfSnapOrNot();
    }


    void RotateSelection(float angleToRotate)
    {
        currentYRotationAdded += angleToRotate;
        gameObjectToEdit.transform.Rotate(Vector3.up, angleToRotate);
        snapGO.transform.Rotate(Vector3.up, angleToRotate);
    }

    void ScaleSelection(float scaleFactor)
    {
        currentScaleAdded += scaleFactor;
        gameObjectToEdit.transform.localScale += Vector3.one * scaleFactor;
        snapGO.transform.localScale += Vector3.one * scaleFactor;
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

    void Select(DecentralandEntity decentralandEntity)
    {
        if (convertedEntities.ContainsKey(sceneToEdit.sceneData.id+decentralandEntity.entityId))
        {
            DecentrelandEntityToEdit entityEditable = convertedEntities[sceneToEdit.sceneData.id + decentralandEntity.entityId];
            if (entityEditable.isLocked || entityEditable.isSelected) return;
            //if (gameObjectToEdit != null) DeselectObject();

            entityEditable.Select();

            currentYRotationAdded = 0;
            currentScaleAdded = 1;

            selectedEntities.Add(entityEditable);

            //GLTFShape shape = (GLTFShape)entityToEdit.GetSharedComponent(typeof(GLTFShape));
            foreach (DecentrelandEntityToEdit entity in selectedEntities)
            {
                entity.entity.gameObject.transform.SetParent(null);
            }
            gameObjectToEdit.transform.position = GetCenterPointOfSelectedObjects();
            gameObjectToEdit.transform.rotation = Quaternion.Euler(0, 0, 0);
            gameObjectToEdit.transform.localScale = Vector3.one;
            foreach (DecentrelandEntityToEdit entity in selectedEntities)
            {
                entity.entity.gameObject.transform.SetParent(gameObjectToEdit.transform);
            }
        

    

            initialRotation = gameObjectToEdit.transform.rotation;

            SetObjectIfSnapOrNot();



            entityInformationController.Enable();
            entityInformationController.SetEntity(decentralandEntity, sceneToEdit);
            CopyGameObjectStatus(gameObjectToEdit, undoGO,false,false);
            CopyGameObjectStatus(gameObjectToEdit, snapGO,false);
          


            sceneLimitInfoController.UpdateInfo();
        }
    }

    Vector3 GetCenterPointOfSelectedObjects()
    {
        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;
        foreach (DecentrelandEntityToEdit entity in selectedEntities)
        {
            totalX += entity.entity.gameObject.transform.position.x;
            totalY += entity.entity.gameObject.transform.position.y;
            totalZ += entity.entity.gameObject.transform.position.z;
        }
        float centerX = totalX / selectedEntities.Count;
        float centerY = totalY / selectedEntities.Count;
        float centerZ = totalZ / selectedEntities.Count;
        return new Vector3(centerX, centerY, centerZ);
    }


    void SetObjectIfSnapOrNot()
    {
        if (!isMultiSelectionActive)
        {
            if (!isSnapActivated)
            {
                Transform originalParent = gameObject.transform.parent;
                gameObjectToEdit.transform.SetParent(null);
                freeMovementGO.transform.position = gameObjectToEdit.transform.position;
                freeMovementGO.transform.rotation = gameObjectToEdit.transform.rotation;
                freeMovementGO.transform.localScale = gameObjectToEdit.transform.localScale;

                gameObjectToEdit.transform.SetParent(originalParent);

                Vector3 pointToLookAt = Camera.main.transform.position;
                pointToLookAt.y = gameObjectToEdit.transform.position.y;
                Quaternion lookOnLook = Quaternion.LookRotation(gameObjectToEdit.transform.position - pointToLookAt);

                freeMovementGO.transform.rotation = lookOnLook;
                gameObjectToEdit.transform.SetParent(freeMovementGO.transform, true);

            }
            else
            {
                gameObjectToEdit.transform.SetParent(null);
            }
        }
    }


    void SelectObject()
    {
        DecentralandEntity entityToSelect = GetEntityOnPointer();
        if(entityToSelect != null) Select(entityToSelect); 
    }

    DecentralandEntity GetEntityOnPointer()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out hit, distanceLimitToSelectObjects, layerToRaycast))
        {
            string entityID = hit.collider.gameObject.name;
            if(sceneToEdit.entities.ContainsKey(entityID)) return sceneToEdit.entities[entityID];
        }
        return null;
    }

    void DeselectEntities()
    {
        if (selectedEntities.Count > 0)
        {
            bool areAllEntitiesInsideSceneBoundaries = true;
            foreach(DecentrelandEntityToEdit entity in selectedEntities)
            {
                if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.entity))
                {              
                    areAllEntitiesInsideSceneBoundaries = false;
                }
               
             
            }

            if(!areAllEntitiesInsideSceneBoundaries) UndoEdit();


            foreach (DecentrelandEntityToEdit entity in selectedEntities)
            {
                SceneController.i.boundariesChecker.EvaluateEntityPosition(entity.entity);
                SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entity.entity);
                entity.Deselect();
            }

            selectedEntities.Clear();
            entityInformationController.Disable();
        }
   
    }

    public void DeletedSelectedEntities()
    {
        List<string> idsToRemove = new List<string>();

        for (int i = 0; i < selectedEntities.Count; i++)
        {         
            idsToRemove.Add(selectedEntities[i].entity.entityId);          
        }

        DeselectEntities();

        foreach(string id in idsToRemove)
        {
            sceneToEdit.RemoveEntity(id, true);
        }
   
    }
    public void DuplicateEntities()
    {
        foreach(DecentrelandEntityToEdit entity in selectedEntities)
        {
            if (!SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entity.entity)) return;
        }

        int amount = selectedEntities.Count;
        for (int i = 0; i < amount; i++)
        {
            DecentralandEntity entity = sceneToEdit.DuplicateEntity(selectedEntities[i].entity);
            SetupEntityToEdit(entity);
            //Select(entity);
        }

    }

    DecentralandEntity CreateEntity()
    {

        DecentralandEntity newEntity = sceneToEdit.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(Camera.main.transform.position + Camera.main.transform.forward * distanceFromCameraForNewEntitties, sceneToEdit);

        Vector3 pointToLookAt = Camera.main.transform.position;
        pointToLookAt.y = gameObjectToEdit.transform.position.y;
        Quaternion lookOnLook = Quaternion.LookRotation(gameObjectToEdit.transform.position - pointToLookAt);

        DCLTransform.model.rotation = lookOnLook;
        DCLTransform.model.scale = newEntity.gameObject.transform.lossyScale;

        sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);      


        sceneLimitInfoController.UpdateInfo();
        SetupEntityToEdit(newEntity,true);
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

        if(isSnapActivated) newEntity.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        Select(newEntity);

        sceneLimitInfoController.UpdateInfo();
    }


    

    void CopyGameObjectStatus(GameObject gameObjectToCopy, GameObject gameObjectToReceive, bool copyParent = true,bool localRotation = true)
    {
        if(copyParent)gameObjectToReceive.transform.SetParent(gameObjectToCopy.transform.parent);
        gameObjectToReceive.transform.position = gameObjectToCopy.transform.position;
        if(localRotation)gameObjectToReceive.transform.localRotation = gameObjectToCopy.transform.localRotation;
        else gameObjectToReceive.transform.rotation = gameObjectToCopy.transform.rotation;
        gameObjectToReceive.transform.localScale = gameObjectToCopy.transform.lossyScale;
    }


    void EnterEditMode()
    {
        
        editModeChangeFX.SetActive(true);
        DCLCharacterController.i.SetFreeMovementActive(true);
        isEditModeActivated = true;
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

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


        SetSnapActive(isSnapActivated);
        FindSceneToEdit();
        sceneToEdit.SetEditMode(true);
        sceneLimitInfoController.SetParcelScene(sceneToEdit);
        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Stop();
        //
        SetupAllEntities();
    }


    void ExitEditMode()
    {
        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Start();
        //


        editModeChangeFX.SetActive(false);

        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;
        DCLCharacterController.i.SetFreeMovementActive(false);
        outlinerController.CancelAllOutlines();
        DeselectEntities();
        isEditModeActivated = false;
        sceneToEdit.SetEditMode(false);
    }


    void FindSceneToEdit()
    {      
        foreach(ParcelScene scene in SceneController.i.scenesSortedByDistance)
        {
            if(scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
            {
                sceneToEdit = scene;
                break;
            }
        }
     
    }

    void DestroyCollidersForAllEntities()
    {
        foreach(DecentrelandEntityToEdit entity in convertedEntities.Values)
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
        if (!convertedEntities.ContainsKey(entity.scene.sceneData.id + entity.entityId))
        {
           
            DecentrelandEntityToEdit entityToEdit = new DecentrelandEntityToEdit(entity, editMaterial);
            convertedEntities.Add(entityToEdit.entityUniqueId, entityToEdit);
            entity.OnRemoved += RemoveConvertedEntity;
            entityToEdit.isNew = hasBeenCreated;
        }

    }

    void RemoveConvertedEntity(DecentralandEntity entity)
    {
        convertedEntities.Remove(entity.scene.sceneData.id + entity.entityId);
    }

}
