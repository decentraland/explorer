using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DCL.Controllers;
using DCL;
using DCL.Configuration;
using DCL.Helpers;
using Builder;

public class AdvancedBuildModeController : MonoBehaviour
{
    public float distanceEagleCamera = 20f;
    public FreeCameraMovement freeCameraController;
    public GameObject eagleCamera;

    //public CameraController cameraController;
    public Transform lookAtT;
    public MouseCatcher mouseCatcher;


    ParcelScene sceneToEdit;
    GameObject goToLookAt;

    public void ActivateAdvancedBuildMode(ParcelScene _sceneToEdit)
    {
        sceneToEdit = _sceneToEdit;

        SetLookAtObject();


        freeCameraController.SetPosition(Camera.main.transform.position + Vector3.up * distanceEagleCamera);
        LookAtTransfrom();

        eagleCamera.gameObject.SetActive(true);
        mouseCatcher.enabled = false;
        SceneController.i.IsolateScene(sceneToEdit);
        Utils.UnlockCursor();
    }

    [ContextMenu("Look at transform")]
    public void LookAtTransfrom()
    {
        freeCameraController.LookAt(lookAtT);
    }
    public void DesactivateAdvancedBuildMode()
    {
        mouseCatcher.enabled = true;
        Utils.LockCursor();
        eagleCamera.gameObject.SetActive(false);
        SceneController.i.ReIntegrateIsolatedScene();
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

        position.x += ParcelSettings.PARCEL_SIZE * Mathf.Abs(maxX - minX) / 2;
        position.z += ParcelSettings.PARCEL_SIZE * Mathf.Abs(maxY - minY) / 2;

        return position;
    }
}
