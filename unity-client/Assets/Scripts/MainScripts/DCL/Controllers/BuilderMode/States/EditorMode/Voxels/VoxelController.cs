using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelController : MonoBehaviour
{
    public BuilderInputWrapper builderInputWrapper;
    public BuildModeController buildModeController;
    public BuildEditorMode buildEditorMode;
    public FreeCameraMovement freeCameraMovement;
    public LayerMask groundLayer;

    DecentralandEntityToEdit lastVoxelCreated;

    GameObject editionGO;
    bool mousePressed = false, isVoxelModelActivated = false;
    Vector3Int lastMousePositionPressed;
    Dictionary<Vector3Int, GameObject> createdVoxels = new Dictionary<Vector3Int, GameObject>();

    private void Start()
    {
        builderInputWrapper.OnMouseDown += MouseDown;
        builderInputWrapper.OnMouseUp += MouseUp;
 
    }

    int cont = 0;
    private void Update()
    {
        if(mousePressed && isVoxelModelActivated)
        {
            if (cont >= 10)
            {
                bool fillVoxels = false;
                Vector3Int currentPosition = Vector3Int.zero;
                VoxelEntityHit voxelHit = buildModeController.GetCloserUnselectedVoxelEntityOnPointer();
                if (voxelHit != null && voxelHit.entityHitted.tag == "Voxel" && !voxelHit.entityHitted.IsSelected)
                {

                    //Vector3Int position = ConverPositionToVoxelPosition(entityToEdit.rootEntity.gameObject.transform.position);    
                    //position.y += 1;
                    Vector3Int position = ConverPositionToVoxelPosition(voxelHit.entityHitted.rootEntity.gameObject.transform.position);
                    position += voxelHit.hitVector;

                    currentPosition = position;
                    fillVoxels = true;
                }
                else
                {
                    RaycastHit hit;
                    UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit, 9999, groundLayer))
                    {
                        currentPosition = ConverPositionToVoxelPosition(hit.point);
                        fillVoxels = true;

                    }
                }
                if(fillVoxels)
                {
                    FillVoxels(lastMousePositionPressed, currentPosition);
                }
                cont = 0;
            }
            else cont++;
        }
    }

    public void SetEditObjectLikeVoxel()
    {
        if (!mousePressed && isVoxelModelActivated)
        {
            VoxelEntityHit voxelHit = buildModeController.GetCloserUnselectedVoxelEntityOnPointer();

            //TODO: Instead of get the entity of the pointer we should get all the enteties and get the entity who is more close to the camera and is not selected
            if (voxelHit != null && voxelHit.entityHitted.IsSelected) return;
            if (voxelHit != null && voxelHit.entityHitted.tag == "Voxel")
            {
                //Vector3 position = voxelHit.entityHitted.rootEntity.gameObject.transform.position;
                //position.x = Mathf.Ceil(position.x);
                //position.y = Mathf.Ceil(position.y) + 1;
                //position.z = Mathf.Ceil(position.z);
                //editionGO.transform.position = position;

                Vector3 position = ConverPositionToVoxelPosition(voxelHit.entityHitted.rootEntity.gameObject.transform.position);
                position += voxelHit.hitVector;
                editionGO.transform.position = position;
            }
            else
            {
                RaycastHit hit;
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 9999, groundLayer))
                {
                    Vector3 position = hit.point;
                    editionGO.transform.position = ConverPositionToVoxelPosition(hit.point);
                }
            }
        }
    }

    public void SetEditionGO(GameObject _editionGO)
    {
        editionGO = _editionGO;
    }

    public bool IsActive()
    {
        return isVoxelModelActivated;
    }
    public void SetActiveMode(bool isActive)
    {
        isVoxelModelActivated = isActive;
    }

    void FillVoxels(Vector3Int firstPosition, Vector3Int lastPosition)
    {
        int xDifference = Mathf.Abs(firstPosition.x - lastPosition.x);
        int yDifference = Mathf.Abs(firstPosition.y - lastPosition.y);
        int zDifference = Mathf.Abs(firstPosition.z - lastPosition.z);


        List<Vector3Int> mustContainVoxelList = new List<Vector3Int>();
        List<DecentralandEntityToEdit> voxelEntities = buildModeController.GetAllVoxelsEntities();
        //CreateVoxel(firstPosition);
        //mustContainVoxelList.Add(firstPosition);

     
        for (int x = 0; x <= xDifference; x++)
        {
            int contX = x;
            if (firstPosition.x > lastPosition.x) contX = -contX;

            for (int y = 0; y <= yDifference; y++)
            {
                int contY = y;
                if (firstPosition.y > lastPosition.y) contY = -contY;

                for (int z = 0; z <= zDifference; z++)
                {
                    int contZ = z;
                    if (firstPosition.z > lastPosition.z) contZ = -contZ;

                    Vector3Int positionOfVoxel = new Vector3Int(firstPosition.x + contX, firstPosition.y + contY, firstPosition.z + contZ);
                    if (positionOfVoxel == firstPosition) continue;
                    if (ExistVoxelAtPosition(positionOfVoxel,voxelEntities)) continue;
                    CreateVoxel(positionOfVoxel);
                    mustContainVoxelList.Add(positionOfVoxel);
                }
            }
        }


        List<Vector3Int> voxelToRemove = new List<Vector3Int>();
        foreach(Vector3Int position in createdVoxels.Keys)
        {
            if (!mustContainVoxelList.Contains(position)) voxelToRemove.Add(position);
        }

        foreach(Vector3Int vector in voxelToRemove)
        {
            Destroy(createdVoxels[vector]);
            createdVoxels.Remove(vector);
        }

    }
    bool ExistVoxelAtPosition(Vector3Int position,List<DecentralandEntityToEdit> voxelEntities)
    {
        foreach (DecentralandEntityToEdit voxelEntity in voxelEntities)
        {
            if (position == ConverPositionToVoxelPosition(voxelEntity.transform.position)) return true;
        }
        return false;
    }
    void CreateVoxel(Vector3Int position)
    {
        if (!createdVoxels.ContainsKey(position))
        {
          
            GameObject go = Instantiate(lastVoxelCreated.rootEntity.meshesInfo.meshRootGameObject, position, lastVoxelCreated.rootEntity.gameObject.transform.rotation);
            createdVoxels.Add(position, go);
        }

    }
    private void MouseUp(int buttonID, Vector3 position)
    {
        if (mousePressed && buttonID == 0)
        {
            lastVoxelCreated.transform.SetParent(null);
            mousePressed = false;
            freeCameraMovement.SetCameraCanMove(true);
            foreach (Vector3Int voxelPosition in createdVoxels.Keys)
            {
                DecentralandEntity entity = buildModeController.DuplicateEntity(lastVoxelCreated);
                entity.gameObject.tag = "Voxel";
                entity.gameObject.transform.position = voxelPosition;
                Destroy(createdVoxels[voxelPosition]);
            }
            createdVoxels.Clear();
            buildModeController.DeselectEntities();
     
     
            lastVoxelCreated = null;
        }
    }
    void MouseDown(int buttonID, Vector3 position)
    {
        if (isVoxelModelActivated && lastVoxelCreated != null && buttonID == 0)
        {
            mousePressed = true;
            freeCameraMovement.SetCameraCanMove(false);

            lastMousePositionPressed = ConverPositionToVoxelPosition(lastVoxelCreated.transform.position);
        }
    }

    public void SetVoxelSelected(DecentralandEntityToEdit decentralandEntityToEdit)
    {
        lastVoxelCreated = decentralandEntityToEdit;
        lastVoxelCreated.transform.localPosition = Vector3.zero;
        //lastVoxelCreated.transform.position = ConverPositionToVoxelPosition(lastVoxelCreated.transform.position);
    }


    public Vector3Int ConverPositionToVoxelPosition(Vector3 rawPosition)
    {
        Vector3Int position = Vector3Int.zero;
        position.x = Mathf.CeilToInt(rawPosition.x);
        position.y = Mathf.CeilToInt(rawPosition.y);
        position.z = Mathf.CeilToInt(rawPosition.z);
        return position;
    }
    //Vector3 GetConvertedVoxelPositionAt(Vector3 position)
    //{
    //    Vector3 result = Vector3.zero;
    //    RaycastHit hit;
    //    UnityEngine.Ray ray = Camera.main.ScreenPointToRay(position);

    //    if (Physics.Raycast(ray, out hit, 9999, groundLayer))
    //    {
    //        Vector3 convertedPosition = hit.point;
    //        result.x = Mathf.CeilToInt(convertedPosition.x);
    //        result.y = Mathf.CeilToInt(convertedPosition.y);
    //        result.z = Mathf.CeilToInt(convertedPosition.z);
    //    }
    //    return result;
    //}
}
