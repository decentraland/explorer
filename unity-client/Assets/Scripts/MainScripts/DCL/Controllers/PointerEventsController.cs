using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using DCL.Interface;
using System.Collections;

namespace DCL
{
    public class PointerEventsController : Singleton<PointerEventsController>
    {
        public static bool renderingIsDisabled = true;

        bool isTesting = false;
        OnPointerUpComponent pointerUpEvent;
        IRaycastHandler raycastHandler = new RaycastHandler();
        Camera charCamera;
        OnPointerEventComponent lastHoveredObject = null;
        OnPointerEventComponent newHoveredObject = null;
        Coroutine hoverInteractiveObjectsRoutine;

        public void Initialize(bool isTesting = false)
        {
            this.isTesting = isTesting;

            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.POINTER, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.PRIMARY, OnButtonEvent);
            InputController_Legacy.i.AddListener(WebInterface.ACTION_BUTTON.SECONDARY, OnButtonEvent);

            RetrieveCamera();

            if (hoverInteractiveObjectsRoutine == null)
                hoverInteractiveObjectsRoutine = SceneController.i.StartCoroutine(HoverInteractiveObjects());
        }

        IEnumerator HoverInteractiveObjects()
        {
            RaycastHit hitInfo;

            while (true)
            {
                if (RenderingController.i.renderingEnabled)
                {
                    // We use Physics.Raycast() instead of our raycastHandler.Raycast() as that one is slower, sometimes 2x, because it fetches info we don't need here
                    if (Physics.Raycast(GetRayFromCamera(), out hitInfo, Mathf.Infinity, Configuration.LayerMasks.physicsCastLayerMaskWithoutCharacter))
                    {
                        newHoveredObject = hitInfo.transform.GetComponentInParent<OnPointerEventComponent>();

                        if (newHoveredObject != null && newHoveredObject.IsAtHoverDistance(hitInfo.distance))
                        {
                            if (newHoveredObject != lastHoveredObject)
                            {
                                UnhoverLastHoveredObject();

                                newHoveredObject.SetHoverState(true);

                                lastHoveredObject = newHoveredObject;
                            }
                        }
                        else
                        {
                            UnhoverLastHoveredObject();
                        }
                    }
                    else
                    {
                        UnhoverLastHoveredObject();
                    }
                }

                yield return null;
            }
        }

        void UnhoverLastHoveredObject()
        {
            if (lastHoveredObject == null) return;

            lastHoveredObject.SetHoverState(false);
            lastHoveredObject = null;
        }

        void RetrieveCamera()
        {
            if (charCamera == null)
            {
                charCamera = Camera.main;
            }
        }

        public Ray GetRayFromCamera()
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

                var pointerEventLayer = Configuration.LayerMasks.physicsCastLayerMaskWithoutCharacter; //Ensure characterController is being filtered
                var globalLayer = ~Configuration.LayerMasks.physicsCastLayerMask & (~Configuration.LayerMasks.characterControllerLayer); //Ensure characterController is being filtered

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