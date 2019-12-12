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
                    RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

                    // Raycast for global pointer events
                    RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);

                    bool isOnClickComponentBlocked =
                        raycastInfoGlobalLayer.hitInfo != null
                        && raycastInfoPointerEventLayer.hitInfo != null
                        && raycastInfoGlobalLayer.hitInfo.hit.distance <= raycastInfoPointerEventLayer.hitInfo.hit.distance
                        && raycastInfoGlobalLayer.hitInfo.collider.entityId != raycastInfoPointerEventLayer.hitInfo.collider.entityId;

                    if (!isOnClickComponentBlocked && raycastInfoPointerEventLayer.hitInfo.hit.rigidbody)
                    {
                        GameObject go = raycastInfoPointerEventLayer.hitInfo.hit.rigidbody.gameObject;

                        go.GetComponentInChildren<OnClickComponent>()?.Report(buttonId);
                        go.GetComponentInChildren<OnPointerDownComponent>()?.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit);

                        pointerUpEvent = go.GetComponentInChildren<OnPointerUpComponent>();
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    if (useRaycast && raycastInfoGlobalLayer.hitInfo.isValid)
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(
                            buttonId,
                            raycastInfoGlobalLayer.ray,
                            raycastInfoGlobalLayer.hitInfo.hit.point,
                            raycastInfoGlobalLayer.hitInfo.hit.normal,
                            raycastInfoGlobalLayer.hitInfo.hit.distance,
                            sceneId,
                            raycastInfoGlobalLayer.hitInfo.collider.entityId,
                            raycastInfoGlobalLayer.hitInfo.collider.meshName,
                            isHitInfoValid: true);
                    }
                    else
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerDownEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneId);
                    }
                }
                else if (evt == InputController_Legacy.EVENT.BUTTON_UP)
                {
                    Ray ray;
                    ray = GetRayFromCamera();

                    // Raycast for global pointer events
                    RaycastResultInfo raycastInfoGlobalLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, globalLayer, null);

                    if (pointerUpEvent != null)
                    {
                        // Raycast for pointer event components
                        RaycastResultInfo raycastInfoPointerEventLayer = raycastHandler.Raycast(ray, charCamera.farClipPlane, pointerEventLayer, null);

                        bool isOnClickComponentBlocked =
                            raycastInfoGlobalLayer.hitInfo != null
                            && raycastInfoPointerEventLayer.hitInfo != null
                            && raycastInfoGlobalLayer.hitInfo.hit.distance <= raycastInfoPointerEventLayer.hitInfo.hit.distance
                            && raycastInfoGlobalLayer.hitInfo.collider.entityId != raycastInfoPointerEventLayer.hitInfo.collider.entityId;

                        if (!isOnClickComponentBlocked)
                        {
                            bool isHitInfoValid = raycastInfoPointerEventLayer.hitInfo.hit.collider != null;
                            pointerUpEvent.Report(buttonId, ray, raycastInfoPointerEventLayer.hitInfo.hit, isHitInfoValid);
                        }

                        pointerUpEvent = null;
                    }

                    string sceneId = SceneController.i.GetCurrentScene(DCLCharacterController.i.characterPosition);

                    if (useRaycast && raycastInfoGlobalLayer.hitInfo.isValid)
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(
                            buttonId,
                            raycastInfoGlobalLayer.ray,
                            raycastInfoGlobalLayer.hitInfo.hit.point,
                            raycastInfoGlobalLayer.hitInfo.hit.normal,
                            raycastInfoGlobalLayer.hitInfo.hit.distance,
                            sceneId,
                            raycastInfoGlobalLayer.hitInfo.collider.entityId,
                            raycastInfoGlobalLayer.hitInfo.collider.meshName,
                            isHitInfoValid: true);
                    }
                    else
                    {
                        DCL.Interface.WebInterface.ReportGlobalPointerUpEvent(buttonId, raycastInfoGlobalLayer.ray, Vector3.zero, Vector3.zero, 0, sceneId);
                    }
                }
            }
        }
    }
}
