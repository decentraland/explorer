using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

namespace DCL
{
    public class PointerEventsController : Singleton<PointerEventsController>
    {
        private LayerMask layerMaskTarget;
        private static int characterControllerLayer => 1 << LayerMask.NameToLayer("CharacterController");

        public static bool renderingIsDisabled = true;
        private OnPointerUpComponent pointerUpEvent;
        private IRaycastHandler raycastHandler = new RaycastHandler();
        private Camera charCamera;
        private bool isTesting = false;

        public PointerEventsController()
        {
            layerMaskTarget = 1 << LayerMask.NameToLayer("OnPointerEvent");
        }

        public void Initialize(bool isTesting = false)
        {
            Debug.Log("isTesting = " + isTesting);
            this.isTesting = isTesting;

            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);

            RetrieveCamera();
        }

        public void Cleanup()
        {
            InputController_Legacy.i.RemoveListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController_Legacy.i.RemoveListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController_Legacy.i.RemoveListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);
        }

        private void RetrieveCamera()
        {
            if (charCamera == null)
            {
                charCamera = Camera.main;
            }
        }

        private Ray GetRayFromCamera()
        {
            return charCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }

        void OnButtonEvent(WebInterface.ACTION_BUTTON buttonId, InputController_Legacy.EVENT evt, bool useRaycast)
        {
            if (Cursor.lockState != CursorLockMode.None && !renderingIsDisabled || this.isTesting)
            {
                if (charCamera == null)
                {
                    RetrieveCamera();

                    if (charCamera == null)
                        return;
                }

                var pointerEventLayer = layerMaskTarget & (~characterControllerLayer); //Ensure characterController is being filtered
                var globalLayer = ~layerMaskTarget & (~characterControllerLayer); //Ensure characterController is being filtered

                if (evt == InputController_Legacy.EVENT.BUTTON_DOWN)
                {
                    Ray ray;

                    ray = GetRayFromCamera();

                    // Raycast for pointer event components
                    RaycastResultInfo raycastInfo = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

                    if (raycastInfo.hitInfo.hit.rigidbody)
                    {
                        GameObject go = raycastInfo.hitInfo.hit.rigidbody.gameObject;

                        go.GetComponentInChildren<OnClickComponent>()?.Report(buttonId);
                        go.GetComponentInChildren<OnPointerDownComponent>()?.Report(buttonId, ray, raycastInfo.hitInfo.hit);

                        pointerUpEvent = go.GetComponentInChildren<OnPointerUpComponent>();
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    // Raycast for global pointer events
                    raycastInfo = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);

                    if (useRaycast && raycastInfo.hitInfo.isValid)
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(
                            buttonId,
                            raycastInfo.ray,
                            raycastInfo.hitInfo.hit.point,
                            raycastInfo.hitInfo.hit.normal,
                            raycastInfo.hitInfo.hit.distance,
                            sceneId,
                            raycastInfo.hitInfo.collider.entityId,
                            raycastInfo.hitInfo.collider.meshName,
                            isHitInfoValid: true);
                    }
                    else
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(buttonId, raycastInfo.ray, Vector3.zero, Vector3.zero, 0, sceneId);
                    }
                }
                else if (evt == InputController_Legacy.EVENT.BUTTON_UP)
                {
                    Ray ray;
                    ray = GetRayFromCamera();

                    // Raycast for pointer event components
                    RaycastResultInfo raycastInfo = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

                    if (pointerUpEvent != null)
                    {
                        bool isHitInfoValid = raycastInfo.hitInfo.hit.collider != null;
                        pointerUpEvent.Report(buttonId, ray, raycastInfo.hitInfo.hit, isHitInfoValid);

                        pointerUpEvent = null;
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    // Raycast for global pointer events
                    raycastInfo = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);

                    if (useRaycast && raycastInfo.hitInfo.isValid)
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(
                            buttonId,
                            raycastInfo.ray,
                            raycastInfo.hitInfo.hit.point,
                            raycastInfo.hitInfo.hit.normal,
                            raycastInfo.hitInfo.hit.distance,
                            sceneId,
                            raycastInfo.hitInfo.collider.entityId,
                            raycastInfo.hitInfo.collider.meshName,
                            isHitInfoValid: true);
                    }
                    else
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(buttonId, raycastInfo.ray, Vector3.zero, Vector3.zero, 0, sceneId);
                    }
                }
            }
        }
    }
}
