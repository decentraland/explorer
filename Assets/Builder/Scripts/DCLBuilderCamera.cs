﻿using UnityEngine;
using DCL.Controllers;
using DCL.Models;
using Builder.Gizmos;

namespace Builder
{
    public class DCLBuilderCamera : MonoBehaviour
    {

        public static System.Action<Camera, float> OnCameraZoomChanged;

        [Header("References")]
        public Transform pitchPivot;
        public Transform yawPivot;
        public Transform rootPivot;
        public Camera builderCamera;

        [Space()]
        [Header("Input Settings")]
        public float rotationSpeed = 5f;
        [Header("Pitch")]
        public float pitchAmount = 10f;
        public float pitchAngleMin = 0f;
        public float pitchAngleMax = 89f;
        [Header("Yaw")]
        public float yawAmount = 10f;
        [Header("Pan")]
        public float panSpeed = 5f;
        public float panAmount = 0.2f;
        [Header("Zoom")]
        public float zoomMin = 1f;
        public float zoomMax = 60f;
        public float zoomSpeed = 5f;
        public float zoomAmount = 0.5f;

        private float pitchCurrent = 0;
        private float pitchTarget = 0;
        private float yawCurrent = 0;
        private float yawTarget = 0;
        private Vector3 panCurrent = Vector3.zero;
        private Vector3 panTarget = Vector3.zero;
        private float zoomCurrent = 0;
        private float zoomTarget = 0;

        private float zoomDefault = 0;

        private bool isObjectBeingDrag = false;

        private Vector2 sceneBoundaryMin;
        private Vector2 sceneBoundaryMax;

        private bool isGameObjectActive = false;

        private void Awake()
        {
            zoomDefault = zoomCurrent = zoomTarget = Mathf.Clamp(builderCamera.transform.localPosition.z, -zoomMax, -zoomMin);
            pitchCurrent = pitchTarget = pitchPivot.localEulerAngles.x;
            yawCurrent = yawTarget = yawPivot.localEulerAngles.y;

            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
        }

        private void OnDestroy()
        {
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void Update()
        {
            yawCurrent += (yawTarget - yawCurrent) * Time.deltaTime * rotationSpeed;
            yawPivot.localRotation = Quaternion.Euler(0, yawCurrent, 0);

            pitchCurrent += (pitchTarget - pitchCurrent) * Time.deltaTime * rotationSpeed;
            pitchPivot.localRotation = Quaternion.Euler(pitchCurrent, 0, 0);

            float zoomPrev = zoomCurrent;
            zoomCurrent += (zoomTarget - zoomCurrent) * Time.deltaTime * zoomSpeed;
            builderCamera.transform.localPosition = new Vector3(0, 0, zoomCurrent);

            Vector3 panOffset = panTarget - panCurrent;
            float sqDist = panOffset.magnitude;
            if (sqDist >= 0.01f)
            {
                panCurrent = panCurrent + panOffset.normalized * sqDist * panSpeed * Time.deltaTime;
                rootPivot.localPosition = panCurrent;
            }
            if (zoomPrev != zoomCurrent)
            {
                OnCameraZoomChanged?.Invoke(builderCamera, zoomCurrent);
            }
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderInput.OnMouseDrag += OnMouseDrag;
                DCLBuilderInput.OnMouseWheel += OnMouseWheel;
                DCLBuilderBridge.OnSetArrowKeyDown += OnKeyboardButtonHold;
                DCLBuilderBridge.OnZoomFromUI += OnZoomFormUI;
                DCLBuilderBridge.OnSetCameraPosition += OnSetCameraPosition;
                DCLBuilderBridge.OnSetCameraRotation += OnSetCameraRotation;
                DCLBuilderBridge.OnResetCameraZoom += OnResetCameraZoom;
                DCLBuilderObjectSelector.OnDraggingObjectStart += OnDragObjectStart;
                DCLBuilderObjectSelector.OnDraggingObjectEnd += OnDragObjectEnd;
                DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmoTransformObjectStart;
                DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnd;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseWheel -= OnMouseWheel;
            DCLBuilderBridge.OnSetArrowKeyDown -= OnKeyboardButtonHold;
            DCLBuilderBridge.OnZoomFromUI -= OnZoomFormUI;
            DCLBuilderBridge.OnSetCameraPosition -= OnSetCameraPosition;
            DCLBuilderBridge.OnSetCameraRotation -= OnSetCameraRotation;
            DCLBuilderBridge.OnResetCameraZoom -= OnResetCameraZoom;
            DCLBuilderObjectSelector.OnDraggingObjectStart -= OnDragObjectStart;
            DCLBuilderObjectSelector.OnDraggingObjectEnd -= OnDragObjectEnd;
            DCLBuilderGizmoManager.OnGizmoTransformObjectStart -= OnGizmoTransformObjectStart;
            DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnd;
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0)
            {
                if (CanOrbit())
                {
                    yawTarget += axisX * yawAmount;
                    pitchTarget = Mathf.Clamp(pitchTarget - axisY * pitchAmount, pitchAngleMin, pitchAngleMax);
                }
            }
            else if (buttonId == 1)
            {
                Pan(0, -axisX * panAmount, -axisY * panAmount);
            }
        }

        private void OnMouseWheel(float axisValue)
        {
            Zoom(axisValue * zoomAmount);
        }

        private void OnKeyboardButtonHold(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.UpArrow:
                    Pan(panAmount, 0, 0);
                    break;
                case KeyCode.DownArrow:
                    Pan(-panAmount, 0, 0);
                    break;
                case KeyCode.LeftArrow:
                    Pan(0, -panAmount, 0);
                    break;
                case KeyCode.RightArrow:
                    Pan(0, panAmount, 0);
                    break;
            }
        }

        private void OnZoomFormUI(float delta)
        {
            Zoom(-delta);
        }

        private void Pan(float forward, float right, float up)
        {
            Vector3 panOffset = ((yawPivot.right * right) + (pitchPivot.up * up) + (rootPivot.forward * forward));
            panTarget += panOffset;
        }

        private void Zoom(float amount)
        {
            zoomTarget = Mathf.Clamp(zoomTarget + amount, -zoomMax, -zoomMin);
        }

        private void OnDragObjectStart(DCLBuilderEntity entity, Vector3 objectPosition)
        {
            isObjectBeingDrag = true;
        }

        private void OnDragObjectEnd(DCLBuilderEntity entity, Vector3 objectPosition)
        {
            isObjectBeingDrag = false;
            pitchTarget = pitchCurrent;
            yawTarget = yawCurrent;
        }

        private void OnGizmoTransformObjectStart(DCLBuilderEntity entity, Vector3 objectPosition, string gizmoType)
        {
            OnDragObjectStart(entity, objectPosition);
        }

        private void OnGizmoTransformObjectEnd(DCLBuilderEntity entity, Vector3 objectPosition, string gizmoType)
        {
            OnDragObjectEnd(entity, objectPosition);
        }

        private void OnSetCameraPosition(Vector3 position)
        {
            panCurrent = position;
            rootPivot.transform.localPosition = panCurrent;
            panTarget = panCurrent;
            pitchCurrent += (pitchTarget - pitchCurrent) * Time.deltaTime * rotationSpeed;
            pitchPivot.localRotation = Quaternion.Euler(pitchCurrent, 0, 0);
        }

        private void OnSetCameraRotation(float yaw, float pitch)
        {
            yawCurrent = yawTarget = yaw;
            pitchCurrent = pitchTarget = pitch;
            yawPivot.localRotation = Quaternion.Euler(0, yawCurrent, 0);
            pitchPivot.localRotation = Quaternion.Euler(pitchCurrent, 0, 0);
        }

        private void OnResetCameraZoom()
        {
            zoomCurrent = zoomTarget = zoomDefault;
            builderCamera.transform.position.Set(0, 0, zoomCurrent);
            OnCameraZoomChanged?.Invoke(builderCamera, zoomCurrent);
        }

        private bool CanOrbit()
        {
            return !isObjectBeingDrag;
        }

        private void CalcSceneBoundaries(ParcelScene scene)
        {
            sceneBoundaryMin = Vector2.zero;
            sceneBoundaryMax = Vector2.zero;

            if (scene != null && scene.sceneData != null)
            {
                CalcSceneBoundaries(scene.sceneData);
            }
        }

        private void CalcSceneBoundaries(LoadParcelScenesMessage.UnityParcelScene scene)
        {
            sceneBoundaryMin = Vector2.zero;
            sceneBoundaryMax = Vector2.zero;

            if (scene.parcels.Length > 0)
            {
                foreach (Vector2Int parcel in scene.parcels)
                {
                    Vector3 parcelPosition = new Vector3(parcel.x, 0, parcel.y);
                    sceneBoundaryMin.x = Mathf.Min(sceneBoundaryMin.x, parcelPosition.x);
                    sceneBoundaryMin.y = Mathf.Min(sceneBoundaryMin.y, parcelPosition.z);
                    sceneBoundaryMax.x = Mathf.Max(sceneBoundaryMax.x, parcelPosition.x);
                    sceneBoundaryMax.y = Mathf.Max(sceneBoundaryMax.y, parcelPosition.z);
                }
            }
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            gameObject.SetActive(!isPreview);
        }
    }
}