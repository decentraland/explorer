using DCL;
using DCL.Controllers;
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


    bool isEditModeActivated = false, isSnapActivated = true;

    GameObject objectToEdit;
    Material originalMaterial;
    Transform originalGOParent;
    MeshRenderer originalMeshRenderer;


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

            if(Input.GetKey(KeyCode.R))
            {
                currentYRotation += rotationSpeed;
                objectToEdit.transform.Rotate(Vector3.up, currentYRotation);
            }

            if(Input.mouseScrollDelta.y >0.5f)
            {
                objectToEdit.transform.localScale += Vector3.one * scaleSpeed;
            }
            else if (Input.mouseScrollDelta.y < -0.5f)
            {
                objectToEdit.transform.localScale -= Vector3.one * scaleSpeed;
            }

                Debug.Log("Current scale "+currentScale+ " Scroll delta "+Input.mouseScrollDelta);
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
        originalMeshRenderer = objectToEdit.GetComponentInChildren<MeshRenderer>();
        originalMaterial = originalMeshRenderer.material;
        originalMeshRenderer.material = editMaterial;

        originalGOParent = objectToEdit.transform.parent;
        objectToEdit.transform.SetParent(Camera.main.transform);

        currentScale = objectToEdit.transform.localScale.magnitude;
        currentYRotation = objectToEdit.transform.eulerAngles.y;
        Debug.Log("Starting editing objet");
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
        DCLCharacterController.i.SetMovementMode(DCLCharacterController.MovementMode.FreeMode);
        isEditModeActivated = true;
    }


    void ExitEditMode()
    {
        isEditModeActivated = false;
        Debug.Log("Exit edit mode");
        editModeChangeFX.SetActive(false);
        DCLCharacterController.i.SetMovementMode(DCLCharacterController.MovementMode.Normal);
        StopEditObject();
    }

}
