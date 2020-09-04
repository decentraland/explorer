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


    [Header("Build References")]

    public Material editMaterial;
    public GameObject editModeChangeFX;
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
        }
    }


 

    void CheckEditModeInput()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (objectToEdit != null) StopEditObject();
            CreateEntity();
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
            if (Input.GetMouseButtonDown(0))
            {
                StopEditObject();
                InputDone();
                return;
            }

            if (Input.GetKey(KeyCode.R))
            {
                currentYRotation += rotationSpeed;
                objectToEdit.transform.Rotate(Vector3.up, rotationSpeed);
            }


            if (Input.mouseScrollDelta.y >0.5f)
            {
                objectToEdit.transform.localScale += Vector3.one * scaleSpeed;
            }
            else if (Input.mouseScrollDelta.y < -0.5f)
            {
                objectToEdit.transform.localScale -= Vector3.one * scaleSpeed;
            }

           
        }
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

        initialRotation = objectToEdit.transform.rotation;

        originalMeshRenderer = objectToEdit.GetComponentInChildren<MeshRenderer>();
        originalMaterial = originalMeshRenderer.material;
        originalMeshRenderer.material = editMaterial;

        originalGOParent = objectToEdit.transform.parent;
        objectToEdit.transform.SetParent(Camera.main.transform);

        currentScale = objectToEdit.transform.localScale.magnitude;
        currentYRotation = objectToEdit.transform.eulerAngles.y;
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


        SelectObject(newEntity);
    }

 

    void StopEditObject()
    {
        if (objectToEdit != null)
        {
            originalMeshRenderer.material = originalMaterial;
            objectToEdit.transform.SetParent(originalGOParent);
            objectToEdit = null;      
            Debug.Log("Stop editing objet");
        }
    }

    void EnterEditMode()
    {
        Debug.Log("Entered edit mode");
        editModeChangeFX.SetActive(true);
        DCLCharacterController.i.SetFreeMovementActive(true);
        isEditModeActivated = true;
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        sceneToEdit = SceneController.i.scenesSortedByDistance[0];
        CreateCollidersForAllEntities();
    }


    void ExitEditMode()
    {
        isEditModeActivated = false;
        Debug.Log("Exit edit mode");
        editModeChangeFX.SetActive(false);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;
        DCLCharacterController.i.SetFreeMovementActive(false);
        StopEditObject();
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
                CreateColliders(entity, entity.meshesInfo);
            }       
        }
    }


    private void CreateColliders(DecentralandEntity entity, DecentralandEntity.MeshesInfo meshInfo)
    {
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
