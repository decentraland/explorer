using UnityEngine;
using DCL.Controllers;
using Builder.Gizmos;
using System.Collections.Generic;

namespace Builder
{
    public class DCLBuilderObjectSelector : MonoBehaviour
    {
        const float MAX_SECS_FOR_CLICK = 0.25f;

        public DCLBuilderRaycast builderRaycast;
        public DCLBuilderGizmoManager gizmosManager;

        public delegate void EntitySelectedDelegate(DCLBuilderEntity entity, string gizmoType);
        public delegate void EntityDeselectedDelegate(DCLBuilderEntity entity);

        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event System.Action OnNoObjectSelected;
        public static event System.Action<List<DCLBuilderEntity>> OnSelectedObjectListChanged;
        public static event System.Action<DCLBuilderEntity, Vector3> OnEntityPressed;

        private List<DCLBuilderEntity> selectedEntities = new List<DCLBuilderEntity>();
        private EntityPressedInfo entityEnqueueForDeselectInfo = new EntityPressedInfo();
        private bool isDirty = false;

        private float groundClickTime = 0;

        private bool isMultiSelectionEnabled = false;

        private bool isGameObjectActive = false;

        private SceneBoundariesChecker boundariesChecker;
        private ParcelScene currentScene;

        private void Awake()
        {
            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
        }

        private void OnDestroy()
        {
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderInput.OnMouseDown += OnMouseDown;
                DCLBuilderInput.OnMouseDrag += OnMouseDrag;
                DCLBuilderInput.OnMouseUp += OnMouseUp;
                DCLBuilderBridge.OnResetObject += OnResetObject;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
                DCLBuilderBridge.OnBuilderSelectEntity += OnBuilderSelectEntity;
                DCLBuilderBridge.OnBuilderDeselectEntity += OnBuilderDeselectEntity;
                DCLBuilderBridge.OnSetKeyDown += OnBuilderKeyDown;
                DCLBuilderBridge.OnSetKeyUp += OnBuilderKeyUp;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDown -= OnMouseDown;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnResetObject -= OnResetObject;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
            DCLBuilderBridge.OnBuilderSelectEntity -= OnBuilderSelectEntity;
            DCLBuilderBridge.OnBuilderDeselectEntity -= OnBuilderDeselectEntity;
            DCLBuilderBridge.OnSetKeyDown -= OnBuilderKeyDown;
            DCLBuilderBridge.OnSetKeyUp -= OnBuilderKeyUp;
        }

        private void Update()
        {
            if (!gizmosManager.isTransformingObject)
            {
                CheckGizmoHover(Input.mousePosition);
            }

            if (isDirty)
            {
                isDirty = false;
                OnSelectedObjectListChanged?.Invoke(selectedEntities);
            }
        }

        private void OnMouseDown(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0)
            {
                RaycastHit hit;
                if (builderRaycast.Raycast(mousePosition, builderRaycast.defaultMask, out hit, true))
                {
                    DCLBuilderGizmoAxis gizmosAxis = hit.collider.gameObject.GetComponent<DCLBuilderGizmoAxis>();
                    if (gizmosAxis != null)
                    {
                        for (int i = 0; i < selectedEntities.Count; i++)
                        {
                            gizmosManager.OnBeginDrag(gizmosAxis, selectedEntities[i]);
                        }
                    }
                    else
                    {
                        var builderSelectionCollider = hit.collider.gameObject.GetComponent<DCLBuilderSelectionCollider>();
                        DCLBuilderEntity pressedEntity = null;

                        if (builderSelectionCollider != null)
                        {
                            pressedEntity = builderSelectionCollider.ownerEntity;
                        }

                        if (pressedEntity != null)
                        {
                            if (CanSelect(pressedEntity))
                            {
                                if (selectedEntities.Contains(pressedEntity))
                                {
                                    EnqueueEntityForDeselect(pressedEntity, hit.point);
                                }
                                else
                                {
                                    OnPressedEntity(pressedEntity, hit.point);
                                }
                                OnEntityPressed?.Invoke(pressedEntity, hit.point);
                            }
                        }
                    }
                    groundClickTime = 0;
                }
                else
                {
                    groundClickTime = Time.unscaledTime;
                }
            }
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0)
            {
                if (gizmosManager.isTransformingObject)
                {
                    gizmosManager.OnEndDrag();
                }

                if (entityEnqueueForDeselectInfo.pressedEntity != null)
                {
                    if ((Time.unscaledTime - entityEnqueueForDeselectInfo.pressedTime) < MAX_SECS_FOR_CLICK)
                    {
                        OnPressedEntity(entityEnqueueForDeselectInfo.pressedEntity, entityEnqueueForDeselectInfo.hitPoint);
                    }
                }
                entityEnqueueForDeselectInfo.pressedEntity = null;

                if (groundClickTime != 0 && (Time.unscaledTime - groundClickTime) < MAX_SECS_FOR_CLICK)
                {
                    if (selectedEntities != null)
                    {
                        OnNoObjectSelected?.Invoke();
                    }
                    DeselectAll();
                }
                groundClickTime = 0;
            }
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0)
            {
                bool hasMouseMoved = (axisX != 0 || axisY != 0);
                if (gizmosManager.isTransformingObject)
                {
                    UpdateGizmoAxis(mousePosition);
                }
            }
        }

        private void OnResetObject()
        {
            for (int i = 0; i < selectedEntities.Count; i++)
            {
                selectedEntities[i].transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (selectedEntities.Contains(entity))
            {
                Deselect(entity);
            }
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            boundariesChecker = scene.boundariesChecker;
            currentScene = scene;
        }

        private void OnBuilderSelectEntity(string entityId)
        {
            if (currentScene && currentScene.entities.ContainsKey(entityId))
            {
                DCLBuilderEntity entity = currentScene.entities[entityId].gameObject.GetComponent<DCLBuilderEntity>();
                if (entity && !gizmosManager.isTransformingObject && CanSelect(entity))
                {
                    entity.SetOnShapeLoaded(() =>
                    {
                        Select(entity);
                    });
                }
            }
        }

        private void OnBuilderDeselectEntity()
        {
            DeselectAll();
        }

        private void OnBuilderKeyDown(KeyCode keyCode)
        {
            if (keyCode == KeyCode.LeftShift)
            {
                isMultiSelectionEnabled = true;
            }
        }

        private void OnBuilderKeyUp(KeyCode keyCode)
        {
            if (keyCode == KeyCode.LeftShift)
            {
                isMultiSelectionEnabled = false;
            }
        }

        private bool CanSelect(DCLBuilderEntity entity)
        {
            return entity.hasGizmoComponent;
        }

        private void Select(DCLBuilderEntity entity)
        {
            if (entity != null && !selectedEntities.Contains(entity))
            {
                selectedEntities.Add(entity);
                entity.SetSelectLayer();

                OnSelectedObject?.Invoke(entity, gizmosManager.GetSelectedGizmo());
                isDirty = true;
            }
        }

        private void Deselect(DCLBuilderEntity entity)
        {
            if (selectedEntities.Contains(entity))
            {
                OnDeselectedObject?.Invoke(entity);
                if (entity != null)
                {
                    entity.SetDefaultLayer();
                }
                selectedEntities.Remove(entity);
            }

            if (selectedEntities.Count == 0)
            {
                OnNoObjectSelected?.Invoke();
            }
            isDirty = true;
        }

        private void DeselectAll()
        {
            for (int i = selectedEntities.Count - 1; i >= 0; i--)
            {
                Deselect(selectedEntities[i]);
            }
        }

        private void UpdateGizmoAxis(Vector3 mousePosition)
        {
            Vector3 hit;
            if (gizmosManager.RaycastHit(builderRaycast.GetMouseRay(mousePosition), out hit))
            {
                gizmosManager.OnDrag(hit, mousePosition);
                for (int i = 0; i < selectedEntities.Count; i++)
                {
                    boundariesChecker?.EvaluateEntityPosition(selectedEntities[i].rootEntity);
                }
            }
        }

        private void CheckGizmoHover(Vector3 mousePosition)
        {
            RaycastHit hit;
            if (builderRaycast.RaycastToGizmos(mousePosition, out hit))
            {
                DCLBuilderGizmoAxis gizmoAxis = hit.collider.gameObject.GetComponent<DCLBuilderGizmoAxis>();
                gizmosManager.SetAxisHover(gizmoAxis);
            }
            else
            {
                gizmosManager.SetAxisHover(null);
            }
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            DeselectAll();
            gameObject.SetActive(!isPreview);
        }

        private void EnqueueEntityForDeselect(DCLBuilderEntity pressedEntity, Vector3 hitPoint)
        {
            entityEnqueueForDeselectInfo.pressedEntity = pressedEntity;
            entityEnqueueForDeselectInfo.pressedTime = Time.unscaledTime;
            entityEnqueueForDeselectInfo.hitPoint = hitPoint;
        }

        private void OnPressedEntity(DCLBuilderEntity pressedEntity, Vector3 hitPoint)
        {
            if (CanSelect(pressedEntity))
            {
                if (isMultiSelectionEnabled)
                {
                    if (!selectedEntities.Contains(pressedEntity))
                    {
                        Select(pressedEntity);
                    }
                    else
                    {
                        Deselect(pressedEntity);
                    }
                }
                else
                {
                    DeselectAll();
                    Select(pressedEntity);
                }

                OnSelectedObjectListChanged?.Invoke(selectedEntities);
                isDirty = false;
            }
        }

        private class EntityPressedInfo
        {
            public DCLBuilderEntity pressedEntity;
            public float pressedTime;
            public Vector3 hitPoint;
        }
    }
}