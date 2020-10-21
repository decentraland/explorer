using Builder.Gizmos;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
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


    //public CameraController cameraController;
    public Transform lookAtT;
    public MouseCatcher mouseCatcher;


    ParcelScene sceneToEdit;

    public LayerMask groundLayer;

    bool isPlacingNewObject = false;

    private void Start()
    {
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmosTransformEnd;
        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmosTransformStart;
    }


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
        //createdEntity.gameObject.transform.eulerAngles = Vector3.zero;
        gizmoManager.HideGizmo();
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

    public override void EntityDeselected(DecentralandEntityToEdit entityDeselected)
    {
        base.EntityDeselected(entityDeselected);
        if(selectedEntities.Count <= 0) gizmoManager.HideGizmo();
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
        gizmoManager.SetGizmoType("MOVE");
        if (selectedEntities.Count > 0) ShowGizmos();
        else gizmoManager.HideGizmo();

    }

    public void RotateMode()
    {
        gizmoManager.SetGizmoType("ROTATE");
        if (selectedEntities.Count > 0) ShowGizmos();
        else gizmoManager.HideGizmo();
        
    }
    public void ScaleMode()
    {
        gizmoManager.SetGizmoType("SCALE");
        if (selectedEntities.Count > 0) ShowGizmos();
        else gizmoManager.HideGizmo();
 
    }
    public void FocusGameObject(List<DecentralandEntityToEdit> entitiesToFocus)
    {
        freeCameraController.FocusOnEntities(entitiesToFocus);
    }

    void OnGizmosTransformStart(string gizmoType)
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            TransformActionStarted(entity.rootEntity,gizmoType);
        }
    }
    void OnGizmosTransformEnd(string gizmoType)
    {
        foreach (DecentralandEntityToEdit entity in selectedEntities)
        {
            TransformActionEnd(entity.rootEntity, gizmoType);
        }

        switch(gizmoType)
        {           
            case "MOVE":

                ActionFinish(BuildModeAction.ActionType.MOVE);
                break;
            case "ROTATE":

                ActionFinish(BuildModeAction.ActionType.ROTATE);
                break;
            case "SCALE":
                ActionFinish(BuildModeAction.ActionType.SCALE);
                break;
        }
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
