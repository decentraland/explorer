using Builder.Gizmos;
using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildEditorMode : BuildModeState
{
    public AdvancedBuildModeController advancedBuildModeController;
    public DCLBuilderGizmoManager gizmoManager;
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
        advancedBuildModeController.ActivateAdvancedBuildMode(scene);

        gameObjectToEdit.transform.SetParent(null);
    }
    public override void Desactivate()
    {
        base.Desactivate();
        advancedBuildModeController.DesactivateAdvancedBuildMode();
    }

    public override void StartMultiSelection()
    {
        base.StartMultiSelection();

        snapGO.transform.SetParent(null);
        freeMovementGO.transform.SetParent(null);
    }

    public override void EndMultiSelection()
    {
        base.EndMultiSelection();
    }

    public override void CreatedEntity(DecentralandEntityToEdit createdEntity)
    {
        base.CreatedEntity(createdEntity);
        isPlacingNewObject = true;
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


        if (!isMultiSelectionActive) advancedBuildModeController.LookAtEntity(selectedEntity.rootEntity);

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
            gizmoManager.SetSnapFactor(snapFactor, snapFactor, snapFactor);
        }
        else gizmoManager.SetSnapFactor(0, 0, 0);
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


}
