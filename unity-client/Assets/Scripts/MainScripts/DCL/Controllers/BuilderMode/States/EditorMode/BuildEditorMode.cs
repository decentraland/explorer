using Builder.Gizmos;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildEditorMode : BuildModeState
{
    [Header("Editor Design")]
    public float distanceEagleCamera = 20f;

    [Header("References")]
    public FreeCameraMovement freeCameraController;
    public GameObject eagleCamera, advancedModeUI;
    public DCLBuilderGizmoManager gizmoManager;
    public SceneObjectCatalogController sceneObjectCatalogController;

    public Outline moveOutline, rotateOutline, scaleOutline;

    //public CameraController cameraController;
    public Transform lookAtT;
    public MouseCatcher mouseCatcher;


    ParcelScene sceneToEdit;

    public LayerMask groundLayer;

    bool isPlacingNewObject = false;


    private void Update()
    {
        if (isPlacingNewObject)
        {
            SetEditObjectAtMouse();
        }
    }
    public override void Activate(ParcelScene scene)
    {
        base.Activate(scene);

        sceneToEdit = scene;

        SetLookAtObject();


        // NOTE(Adrian): Take into account that right now to get the relative scale of the gizmos, we set the gizmos in the player position and the camera 
        Vector3 cameraPosition = DCLCharacterController.i.characterPosition.unityPosition;
     
        freeCameraController.SetPosition(cameraPosition + Vector3.up * distanceEagleCamera);

        //
        freeCameraController.LookAt(lookAtT);

        eagleCamera.gameObject.SetActive(true);
        gizmoManager.InitializeGizmos(Camera.main,freeCameraController.transform);
        gizmoManager.SetAllGizmosInPosition(cameraPosition);
        if (gizmoManager.GetSelectedGizmo() == DCL.Components.DCLGizmos.Gizmo.NONE) gizmoManager.SetGizmoType("MOVE");
        mouseCatcher.enabled = false;
        SceneController.i.IsolateScene(sceneToEdit);
        Utils.UnlockCursor();
        advancedModeUI.SetActive(true);
        CommonScriptableObjects.allUIHidden.Set(true);
        RenderSettings.fog = false;
        gizmoManager.HideGizmo();
        gameObjectToEdit.transform.SetParent(null);
    }
    public override void Desactivate()
    {
        base.Desactivate();
        mouseCatcher.enabled = true;
        Utils.LockCursor();
        eagleCamera.gameObject.SetActive(false);
        SceneController.i.ReIntegrateIsolatedScene();
        advancedModeUI.SetActive(false);
        gizmoManager.HideGizmo();
        CommonScriptableObjects.allUIHidden.Set(false);
        RenderSettings.fog = true;
    }

    public override void StartMultiSelection()
    {
        base.StartMultiSelection();

        snapGO.transform.SetParent(null);
        freeMovementGO.transform.SetParent(null);
    }


    public override void CreatedEntity(DecentralandEntityToEdit createdEntity)
    {
        base.CreatedEntity(createdEntity);
        isPlacingNewObject = true;
       
    }
    public override Vector3 GetCreatedEntityPoint()
    {
        return GetFloorPointAtMouse();
    }

    public override void SelectedEntity(DecentralandEntityToEdit selectedEntity)
    {
        base.SelectedEntity(selectedEntity);

        List<EditableEntity> editableEntities = new List<EditableEntity>();
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            editableEntities.Add(entity);
        }

        gizmoManager.SelectedEntities(gameObjectToEdit.transform, editableEntities);

        if (!isMultiSelectionActive && !selectedEntity.IsNew) LookAtEntity(selectedEntity.rootEntity);

        snapGO.transform.SetParent(null);
    }

    public override void DeselectedEntities()
    {
        base.DeselectedEntities();
        gizmoManager.HideGizmo();
        isPlacingNewObject = false;
    }

    public override void SetSnapActive(bool isActive)
    {
        base.SetSnapActive(isActive);

        if (isSnapActive)
        {
            gizmoManager.SetSnapFactor(snapFactor, snapRotationDegresFactor, snapScaleFactor);
        }
        else gizmoManager.SetSnapFactor(0, 0, 0);
    }
    public override void CheckInput()
    {
        base.CheckInput();
        if (Input.GetKey(KeyCode.Alpha1))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(0);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(1);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(2);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(3);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(4);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(5);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha7))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(6);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha8))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(7);
            InputDone();
        }
        if (Input.GetKey(KeyCode.Alpha9))
        {
            sceneObjectCatalogController.FavotiteObjectSelected(8);
            InputDone();
        }
    }
    public override void CheckInputSelectedEntities()
    {
        base.CheckInputSelectedEntities();
        if (Input.GetKey(KeyCode.F))
        {
            FocusGameObject(selectedEntities);
            InputDone();
            return;
        }
  
    }

    public void LookAtEntity(DecentralandEntity entity)
    {
        Vector3 pointToLook = entity.gameObject.transform.position;
        if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0)
        {
            Vector3 midPointFromEntityMesh = Vector3.zero;
            foreach (Renderer render in entity.renderers)
            {
                midPointFromEntityMesh += render.bounds.center;
            }
            midPointFromEntityMesh /= entity.renderers.Length;
            pointToLook = midPointFromEntityMesh;
        }
        freeCameraController.SmoothLookAt(pointToLook);
    }

    public void TranslateMode()
    {
        if (selectedEntities.Count > 0) ShowGizmos();
        moveOutline.enabled = true;
        rotateOutline.enabled = false;
        scaleOutline.enabled = false;
        gizmoManager.SetGizmoType("MOVE");
    }

    public void RotateMode()
    {
        if(selectedEntities.Count > 0) ShowGizmos();
        moveOutline.enabled = false;
        rotateOutline.enabled = true;
        scaleOutline.enabled = false;
        gizmoManager.SetGizmoType("ROTATE");
    }
    public void ScaleMode()
    {
        if (selectedEntities.Count > 0) ShowGizmos();
        moveOutline.enabled = false;
        rotateOutline.enabled = false;
        scaleOutline.enabled = true;
        gizmoManager.SetGizmoType("SCALE");
    }
    public void FocusGameObject(List<DecentralandEntityToEdit> entitiesToFocus)
    {
        freeCameraController.FocusOnEntities(entitiesToFocus);
    }

    void ShowGizmos()
    {
        gizmoManager.ShowGizmo();
    }
    void SetLookAtObject()
    {
        Vector3 middlePoint = CalculateMiddlePoint(sceneToEdit.sceneData.parcels);

        lookAtT.position = SceneController.i.ConvertSceneToUnityPosition(middlePoint);
    }
    Vector3 CalculateMiddlePoint(Vector2Int[] positions)
    {
        Vector3 position;

        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;

        int minX = 9999;
        int minY = 9999;
        int maxX = -9999;
        int maxY = -9999;

        foreach (Vector2Int vector in positions)
        {
            totalX += vector.x;
            totalZ += vector.y;
            if (vector.x < minX) minX = vector.x;
            if (vector.y < minY) minY = vector.y;
            if (vector.x > maxX) maxX = vector.x;
            if (vector.y > maxY) maxY = vector.y;
        }
        float centerX = totalX / positions.Length;
        float centerZ = totalZ / positions.Length;

        position.x = centerX;
        position.y = totalY;
        position.z = centerZ;

        int amountParcelsX = Mathf.Abs(maxX - minX)+1;
        int amountParcelsZ = Mathf.Abs(maxY - minY)+1;

        position.x += ParcelSettings.PARCEL_SIZE/2 * amountParcelsX;
        position.z += ParcelSettings.PARCEL_SIZE/2 * amountParcelsZ;

        return position;
    }



    void SetEditObjectAtMouse()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 9999, groundLayer))
        {
            gameObjectToEdit.transform.position = hit.point;
        }
    }

    Vector3 GetFloorPointAtMouse()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 9999, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

}
