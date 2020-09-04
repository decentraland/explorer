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
    [Header("Design variables")]

    public float scaleSpeed = 0.25f;
    public float rotationSpeed = 0.5f;
    public float msBetweenInputInteraction = 200;

    public float distanceLimitToSelectObjects = 50;


    [Header("Snap variables")]
    public float snapFactor = 1f;
    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;


    [Header ("Scene references")]
    public GameObject editModeChangeFX;
    public GameObject snapActivateGO;

    [Header("Build References")]

    public Material editMaterial;
 
    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChange;


    ParcelScene sceneToEdit;

    bool isEditModeActivated = false, isSnapActivated = true;

    //Object to edit related
    DecentralandEntity entityToEdit,newEntity;
    GameObject objectToEdit;
    Material originalMaterial;
    Transform originalGOParent;
    MeshRenderer originalMeshRenderer;

    Quaternion initialRotation;

    
    Dictionary<string, GameObject> collidersDictionary = new Dictionary<string, GameObject>();

    GameObject undoGameObject,snapGameObject;

    float currentScale, currentYRotation, nexTimeToReceiveInput;
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
        if (objectToEdit != null)
        {
            Vector3 initialRotationVector = objectToEdit.transform.rotation.eulerAngles;

            initialRotationVector.x = 0;
            initialRotationVector.z = 0;
            Quaternion targetRotation = Quaternion.Euler(initialRotationVector);
            objectToEdit.transform.rotation = targetRotation;
            snapGameObject.transform.rotation = targetRotation;

            if (isSnapActivated)
            {
                Vector3 objectPosition = snapGameObject.transform.position;
                Vector3 eulerRotation = snapGameObject.transform.rotation.eulerAngles;
  
                objectPosition.x = Mathf.RoundToInt(objectPosition.x);
                objectPosition.y = Mathf.RoundToInt(objectPosition.y);
                objectPosition.z = Mathf.RoundToInt(objectPosition.z);
                eulerRotation.y = snapRotationDegresFactor * Mathf.FloorToInt((eulerRotation.y % snapRotationDegresFactor));

                objectToEdit.transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, eulerRotation.z);
                objectToEdit.transform.position = objectPosition;

            }
        }
    }


 

    void CheckEditModeInput()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (objectToEdit != null) DeselectObject();
            CreateEntity();
        }

        if(Input.GetKeyUp(KeyCode.T))
        {
            SetSnapActive(!isSnapActivated);
        }

        if (objectToEdit == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectObject();
                InputDone();
                return;
            }
        }
        if (objectToEdit != null)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Z))
            {
                UndoEdit();
                InputDone();
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                DeselectObject();
                InputDone();
                return;
            }

            if (Input.GetKey(KeyCode.R))
            {
                if (isSnapActivated)
                {
                    //currentYRotation += snapRotationDegresFactor;
                    //objectToEdit.transform.Rotate(Vector3.up, snapRotationDegresFactor);
                    //snapGameObject.transform.Rotate(Vector3.up, snapRotationDegresFactor);

                    RotateSelectedGameObject(snapRotationDegresFactor);
                    InputDone();
                }
                else
                {
                    //currentYRotation += rotationSpeed;
                    //objectToEdit.transform.Rotate(Vector3.up, rotationSpeed);
                    //snapGameObject.transform.Rotate(Vector3.up, rotationSpeed);
                    RotateSelectedGameObject(rotationSpeed);
                }
            }


            if (Input.mouseScrollDelta.y >0.5f)
            {

                if (isSnapActivated)
                {
                    //objectToEdit.transform.localScale += Vector3.one * snapScaleFactor;
                    ScaleSelectedGameObject(snapScaleFactor);
                    InputDone();
                }
                else ScaleSelectedGameObject(scaleSpeed); //objectToEdit.transform.localScale += Vector3.one * scaleSpeed;
            }
            else if (Input.mouseScrollDelta.y < -0.5f)
            {
                if (isSnapActivated)
                {
                    //objectToEdit.transform.localScale -= Vector3.one * snapScaleFactor;
                    ScaleSelectedGameObject(-snapScaleFactor);
                    InputDone();
                }
                else ScaleSelectedGameObject(-scaleSpeed); // objectToEdit.transform.localScale -= Vector3.one * scaleSpeed;
            }

           
        }
    }

    public void UndoEdit()
    {   
        CopyGameObjectStatus(undoGameObject, objectToEdit);
        DeselectObject();
    }
    public void SetSnapActive(bool isActive)
    {
        isSnapActivated = isActive;
        snapActivateGO.SetActive(isSnapActivated);

    }


    void RotateSelectedGameObject(float angleToRotate)
    {
        currentYRotation += angleToRotate;
        objectToEdit.transform.Rotate(Vector3.up, angleToRotate);
        snapGameObject.transform.Rotate(Vector3.up, angleToRotate);
    }

    void ScaleSelectedGameObject(float scaleFactor)
    {
        objectToEdit.transform.localScale += Vector3.one * scaleFactor;
        snapGameObject.transform.localScale += Vector3.one * scaleFactor;
    }
    void InputDone()
    {
        nexTimeToReceiveInput = Time.timeSinceLevelLoad+msBetweenInputInteraction/1000;      
    }


    private void OnEditModeChangeAction(DCLAction_Trigger action)
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

    void SelectObject(DecentralandEntity decentralandEntity)
    {
        entityToEdit = decentralandEntity;
        objectToEdit = decentralandEntity.gameObject;

        CopyGameObjectStatus(objectToEdit, undoGameObject);

        initialRotation = objectToEdit.transform.rotation;

        originalMeshRenderer = objectToEdit.GetComponentInChildren<MeshRenderer>();
        originalMaterial = originalMeshRenderer.material;
        originalMeshRenderer.material = editMaterial;

        originalGOParent = objectToEdit.transform.parent;
        objectToEdit.transform.SetParent(Camera.main.transform);

        currentScale = objectToEdit.transform.localScale.magnitude;
        currentYRotation = objectToEdit.transform.eulerAngles.y;

        CopyGameObjectStatus(objectToEdit, snapGameObject);


    
        Debug.Log("Starting editing objet");

    }
    void SelectObject()
    {

        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, distanceLimitToSelectObjects, layerToRaycast))
        {
            string entityID = hit.collider.gameObject.name;
            SelectObject(sceneToEdit.entities[entityID]);
            Debug.Log("Entity hitted " + entityID);
        } 
    }

    void DeselectObject()
    {
        if (objectToEdit != null)
        {
            originalMeshRenderer.material = originalMaterial;
            objectToEdit.transform.SetParent(originalGOParent);
            objectToEdit = null;
            Debug.Log("Stop editing objet");
        }
    }

    void CreateEntity()
    {
      
        newEntity = sceneToEdit.CreateEntity(Guid.NewGuid().ToString());

        DCLTransform.model.position =  SceneController.i.ConvertUnityToScenePosition(Camera.main.transform.position+ Camera.main.transform.forward* 2,sceneToEdit);
        DCLTransform.model.rotation = Camera.main.transform.rotation;
        DCLTransform.model.scale = newEntity.gameObject.transform.localScale;

        sceneToEdit.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.TRANSFORM, DCLTransform.model);

        BaseDisposable mesh = sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.BOX_SHAPE));
        sceneToEdit.SharedComponentAttach(newEntity.entityId, mesh.id);

        BaseDisposable material = sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.PBR_MATERIAL));

        ((PBRMaterial)material).model.albedoColor = editMaterial.color;
        sceneToEdit.SharedComponentAttach(newEntity.entityId, material.id);

        CreateCollidersForEntity(newEntity);
        SelectObject(newEntity);
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
        Debug.Log("Entered edit mode");
        editModeChangeFX.SetActive(true);
        DCLCharacterController.i.SetFreeMovementActive(true);
        isEditModeActivated = true;
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        if (snapGameObject == null)
        {
            snapGameObject = new GameObject("SnapGameObject");
            snapGameObject.transform.SetParent(transform);
        }
        if (undoGameObject == null)
        {
            undoGameObject = new GameObject("UndoGameObject");
            undoGameObject.transform.SetParent(transform);
        }


        SetSnapActive(snapActivateGO);
        sceneToEdit = SceneController.i.scenesSortedByDistance[0];
        CreateCollidersForAllEntities();
    }


    void ExitEditMode()
    {
        isEditModeActivated = false;
        Debug.Log("Exit edit mode");
        editModeChangeFX.SetActive(false);

        snapGameObject.transform.SetParent(transform);
        ParcelSettings.VISUAL_LOADING_ENABLED = true;
        DCLCharacterController.i.SetFreeMovementActive(false);
        DeselectObject();
        DestroyCollidersForAllEntities();
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
        if (meshInfo == null) return;

        if (!collidersDictionary.ContainsKey(entity.entityId))
        {
            GameObject entityCollider = new GameObject(entity.entityId);
            entityCollider.layer = LayerMask.NameToLayer("OnBuilderPointerClick");

            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {                             
                Transform t = entityCollider.transform;
                t.SetParent(meshInfo.renderers[i].transform);
                t.ResetLocalTRS();

                var meshCollider = entityCollider.AddComponent<MeshCollider>();

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
