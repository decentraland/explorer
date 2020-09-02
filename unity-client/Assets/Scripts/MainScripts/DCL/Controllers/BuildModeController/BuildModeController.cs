using DCL;
using DCL.Controllers;
using DCL.Models;
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


    [Header("Scene References")]
  
    public GameObject objectToTest;

    [Header("Build References")]

    public Material editMaterial;
    public GameObject editModeChangeFX;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChange;


    ParcelScene sceneToEdit;

    bool isEditModeActivated = false, isSnapActivated = true;

    //Object to edit related
    DecentralandEntity entityToEdit;
    GameObject objectToEdit;
    Material originalMaterial;
    Transform originalGOParent;
    MeshRenderer originalMeshRenderer;

    Quaternion initialRotation;

    //

    float currentScale, currentYRotation, nexTimeToReceiveInput;
    // Start is called before the first frame update
    void Start()
    {
        //ÑAPA
        SceneController.VERBOSE = true;


        ParcelScene scene = SceneController.i.CreateTestScene(null);
        scene.CreateEntity("TestEntity");

        editModeChange.OnTriggered += OnCameraChangeAction;


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
            //Debug.Log("Euler angle Y" + transform.rotation.y)
            //if (Input.GetKey(KeyCode.E))
            //{
                Vector3 initialRotationVector = objectToEdit.transform.rotation.eulerAngles;
                //initialRotationVector.x = initialRotation.eulerAngles.x;
       
                //initialRotationVector.z = initialRotation.eulerAngles.z;

            initialRotationVector.x =0;
            initialRotationVector.z = 0;
                Quaternion targetRotation = Quaternion.Euler(initialRotationVector);
                objectToEdit.transform.rotation = targetRotation;
            //}

            Debug.Log("global rotation " + objectToEdit.transform.rotation.eulerAngles + "    global position " + objectToEdit.transform.position);
        }
    }


    void CheckEditModeInput()
    {
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
                //Quaternion.R
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


    private void OnCameraChangeAction(DCLAction_Trigger action)
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

    void SelectObject()
    {
        objectToEdit = objectToTest;

        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out hit))
        {
            objectToEdit = hit.collider.gameObject.transform.parent.gameObject;
        
            if (objectToEdit.name.StartsWith("ENTITY_"))
            {
                string entityName = objectToEdit.name.Replace("ENTITY_", "");

                entityToEdit = sceneToEdit.entities[entityName];
                
            }
            else
            {
                objectToEdit = null;
                return;
            }
        }


   
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

    #region Borrar




    #endregion

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

        sceneToEdit = SceneController.i.scenesSortedByDistance[0];

    }


    void ExitEditMode()
    {
        isEditModeActivated = false;
        Debug.Log("Exit edit mode");
        editModeChangeFX.SetActive(false);
        DCLCharacterController.i.SetFreeMovementActive(false);
        StopEditObject();
    }

}
