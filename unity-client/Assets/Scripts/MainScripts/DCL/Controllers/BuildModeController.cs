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


    [Header ("Scene references")]
    public GameObject editModeChangeFX;
    public GameObject snapImgStatusShowGO;

    [Header("Build References")]

    public Material editMaterial;
 
    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChange;


    ParcelScene sceneToEdit;

    bool isEditModeActivated = false, isSnapActivated = true;

    //Object to edit related
    DecentralandEntity entityToEdit,newEntity;
    GameObject gameObjectToEdit;
    Material originalMaterial;
    Transform originalGOParent;
    MeshRenderer originalMeshRenderer;

    Quaternion initialRotation;

    
    Dictionary<string, GameObject> collidersDictionary = new Dictionary<string, GameObject>();

    GameObject undoGO,snapGO,freeMovementGO;

    bool selectionHasbeenCreated = false;
    float currentScaleAdded, currentYRotationAdded, nexTimeToReceiveInput;
    // Start is called before the first frame update
    void Start()
    {
        editModeChange.OnTriggered += OnEditModeChangeAction;

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
            if(Time.timeSinceLevelLoad >= nexTimeToReceiveInput) CheckEditModeInput();          
        }
    }

    void LateUpdate()
    {
        if (gameObjectToEdit != null)
        {
          


            if (isSnapActivated)
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
                gameObjectToEdit.transform.rotation = initialRotation* destinationRotation;
                gameObjectToEdit.transform.position = objectPosition;

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


 

    void CheckEditModeInput()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (gameObjectToEdit != null) DeselectObject();
            CreateEntity();
        }

        if(Input.GetKeyUp(KeyCode.T))
        {
            SetSnapActive(!isSnapActivated);
        }

        if (gameObjectToEdit == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectObject();
                InputDone();
                return;
            }
        }
        if (gameObjectToEdit != null)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Z))
            {
                UndoEdit();
                InputDone();
                return;
            }
            if (Input.GetMouseButtonDown(0) && SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entityToEdit))
            {
                DeselectObject();
                InputDone();
                return;
            }

            if (Input.GetKey(KeyCode.R))
            {
                if (isSnapActivated)
                {

                    RotateSelectedGameObject(snapRotationDegresFactor);
                    InputDone();
                }
                else
                {
                    RotateSelectedGameObject(rotationSpeed);
                }
            }


            if (Input.mouseScrollDelta.y >0.5f)
            {

                if (isSnapActivated)
                {
                    ScaleSelectedGameObject(snapScaleFactor);
                    InputDone();
                }
                else ScaleSelectedGameObject(scaleSpeed);
            }
            else if (Input.mouseScrollDelta.y < -0.5f)
            {
                if (isSnapActivated)
                {
                    ScaleSelectedGameObject(-snapScaleFactor);
                    InputDone();
                }
                else ScaleSelectedGameObject(-scaleSpeed); 
            }

           
        }
    }

    public void UndoEdit()
    {
        if (!selectionHasbeenCreated)
        {
            CopyGameObjectStatus(undoGO, gameObjectToEdit);
            DeselectObject();
        }
        else
        {
            sceneToEdit.RemoveEntity(entityToEdit.entityId, true);
            DeselectObject();
        }
    }
    public void SetSnapActive(bool isActive)
    {
        isSnapActivated = isActive;
        snapImgStatusShowGO.SetActive(isSnapActivated);
        if (gameObjectToEdit != null) gameObjectToEdit.transform.SetParent(Camera.main.transform,true);
    }


    void RotateSelectedGameObject(float angleToRotate)
    {
        currentYRotationAdded += angleToRotate;
        gameObjectToEdit.transform.Rotate(Vector3.up, angleToRotate);
        snapGO.transform.Rotate(Vector3.up, angleToRotate);
    }

    void ScaleSelectedGameObject(float scaleFactor)
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

    void SelectObject(DecentralandEntity decentralandEntity)
    {

        selectionHasbeenCreated = false;
        currentYRotationAdded = 0;
        currentScaleAdded = 1;

        entityToEdit = decentralandEntity;
        gameObjectToEdit = decentralandEntity.gameObject;

        CopyGameObjectStatus(gameObjectToEdit, undoGO);

        initialRotation = gameObjectToEdit.transform.rotation;



        originalMeshRenderer = gameObjectToEdit.GetComponentInChildren<MeshRenderer>();
        originalMaterial = originalMeshRenderer.material;
        originalMeshRenderer.material = editMaterial;

        originalGOParent = gameObjectToEdit.transform.parent;
        if (isSnapActivated) gameObjectToEdit.transform.SetParent(Camera.main.transform);
        else
        {
            
            freeMovementGO.transform.position = gameObjectToEdit.transform.position;
            freeMovementGO.transform.localScale = gameObjectToEdit.transform.localScale;
            Vector3 pointToLookAt = Camera.main.transform.position;
            pointToLookAt.y = gameObjectToEdit.transform.position.y;
            Quaternion lookOnLook = Quaternion.LookRotation(gameObjectToEdit.transform.position - pointToLookAt);

            freeMovementGO.transform.rotation = lookOnLook;
            gameObjectToEdit.transform.SetParent(freeMovementGO.transform, true);
        }


        CopyGameObjectStatus(gameObjectToEdit, snapGO);
        SceneController.i.boundariesChecker.AddPersistent(entityToEdit);
    }
    void SelectObject()
    {

        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, distanceLimitToSelectObjects, layerToRaycast))
        {
            string entityID = hit.collider.gameObject.name;
            SelectObject(sceneToEdit.entities[entityID]);
        } 
    }

    void DeselectObject()
    {
        if (gameObjectToEdit != null)
        {
            if(SceneController.i.boundariesChecker.IsEntityInsideSceneBoundaries(entityToEdit))
            {            
                gameObjectToEdit.transform.SetParent(originalGOParent);
                SceneController.i.boundariesChecker.EvaluateEntityPosition(entityToEdit);
            }
            else
            {
                UndoEdit();
            }
            originalMeshRenderer.material = originalMaterial;
            SceneController.i.boundariesChecker.RemoveEntityToBeChecked(entityToEdit);
            gameObjectToEdit = null;
            entityToEdit = null;
        }
    }

    void CreateEntity()
    {
      
        newEntity = sceneToEdit.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position =  SceneController.i.ConvertUnityToScenePosition(Camera.main.transform.position+ Camera.main.transform.forward* distanceFromCameraForNewEntitties,sceneToEdit);
        DCLTransform.model.rotation = Quaternion.Euler(Vector3.zero);
        DCLTransform.model.scale = newEntity.gameObject.transform.localScale;

        sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);

        BaseDisposable mesh = sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.BOX_SHAPE));
        sceneToEdit.SharedComponentAttach(newEntity.entityId, mesh.id);

        BaseDisposable material = sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.PBR_MATERIAL));

        ((PBRMaterial)material).model.albedoColor = editMaterial.color;
        sceneToEdit.SharedComponentAttach(newEntity.entityId, material.id);

        if(isSnapActivated) newEntity.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        CreateCollidersForEntity(newEntity);
        SelectObject(newEntity);


        selectionHasbeenCreated = true;
    }


    void CopyGameObjectStatus(GameObject gameObjectToCopy, GameObject gameObjectToReceive)
    {
        gameObjectToReceive.transform.SetParent(gameObjectToCopy.transform.parent);
        gameObjectToReceive.transform.position = gameObjectToCopy.transform.position;
        gameObjectToReceive.transform.localRotation = gameObjectToCopy.transform.localRotation;
        gameObjectToReceive.transform.localScale = gameObjectToCopy.transform.localScale;
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
            snapGO.transform.SetParent(transform);
        }
        if (freeMovementGO == null)
        {
            freeMovementGO = new GameObject("FreeMovementGO");
            freeMovementGO.transform.SetParent(Camera.main.transform);
        }
        if (undoGO == null)
        {
            undoGO = new GameObject("UndoGameObject");
            undoGO.transform.SetParent(transform);
        }


        SetSnapActive(isSnapActivated);
        FindSceneToEdit();
        sceneToEdit.SetEditMode(true);
        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Stop();
        //
        CreateCollidersForAllEntities();
    }


    void ExitEditMode()
    {
        // NOTE(Adrian): This is a temporary as the kernel should do this job instead of the client
        DCL.Environment.i.messagingControllersManager.messagingControllers[sceneToEdit.sceneData.id].systemBus.Start();
        //
        isEditModeActivated = false;
        editModeChangeFX.SetActive(false);

        snapGO.transform.SetParent(transform);
        sceneToEdit.SetEditMode(false);
        ParcelSettings.VISUAL_LOADING_ENABLED = true;
        DCLCharacterController.i.SetFreeMovementActive(false);
        DeselectObject();
        DestroyCollidersForAllEntities();
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
        foreach (GameObject entityCollider in collidersDictionary.Values)
        {
            Destroy(entityCollider);
        }
        collidersDictionary.Clear();
    }
    void CreateCollidersForAllEntities()
    {
        foreach(DecentralandEntity entity in sceneToEdit.entities.Values)
        {
            if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0)
            {
                CreateCollidersForEntity(entity);
            }       
        }
    }


    private void CreateCollidersForEntity(DecentralandEntity entity)
    {
        DecentralandEntity.MeshesInfo meshInfo = entity.meshesInfo;
        if (meshInfo == null || meshInfo.currentShape == null ) return;
        if (!meshInfo.currentShape.IsVisible()) return;
        if (!meshInfo.currentShape.IsVisible() && meshInfo.currentShape.HasCollisions()) return;
        if (!meshInfo.currentShape.IsVisible() && !meshInfo.currentShape.HasCollisions()) return;

        if (!collidersDictionary.ContainsKey(entity.entityId))
        {
            if (entity.children.Count > 0)
            {
                using (var iterator = entity.children.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        CreateCollidersForEntity(iterator.Current.Value);
                    }
                }
            }


            GameObject entityCollider = new GameObject(entity.entityId);
            entityCollider.layer = LayerMask.NameToLayer("OnBuilderPointerClick");

            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {                             
                Transform t = entityCollider.transform;
                t.SetParent(meshInfo.renderers[i].transform);
                t.ResetLocalTRS();

                var meshCollider = entityCollider.AddComponent<MeshCollider>();
                //meshCollider.convex = true;
                //meshCollider.isTrigger = true;
                if (meshInfo.renderers[i] is SkinnedMeshRenderer)
                {
                    Mesh meshColliderForSkinnedMesh = new Mesh();
                    (meshInfo.renderers[i] as SkinnedMeshRenderer).BakeMesh(meshColliderForSkinnedMesh);
                    meshCollider.sharedMesh = meshColliderForSkinnedMesh;
                    t.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
                }
                else
                {
                    meshCollider.sharedMesh = meshInfo.renderers[i].GetComponent<MeshFilter>().sharedMesh;
                }
                meshCollider.enabled = meshInfo.renderers[i].enabled;

            }
            
            collidersDictionary.Add(entity.entityId, entityCollider);
        }
    }
}
