﻿using UnityEngine;
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
        public delegate void EntitySelectedListChangedDelegate(Transform selectionParent, List<DCLBuilderEntity> selectedEntities);

        public static event EntitySelectedDelegate OnMarkObjectSelected;
        public static event EntitySelectedDelegate OnMarkObjectDeselected;
        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event System.Action OnNoObjectSelected;
        public static event EntitySelectedListChangedDelegate OnSelectedObjectListChanged;
        public static event System.Action<DCLBuilderEntity, Vector3> OnEntityPressed;
        public static event System.Action<DCLBuilderGizmoAxis> OnGizmosAxisPressed;

        public Transform selectedEntitiesParent { private set; get; }

        private Dictionary<string, DCLBuilderEntity> entities = new Dictionary<string, DCLBuilderEntity>();
        private List<DCLBuilderEntity> selectedEntities = new List<DCLBuilderEntity>();
        private EntityPressedInfo entityEnqueueForDeselectInfo = new EntityPressedInfo();
        private bool isDirty = false;
        private bool isSelectionTransformed = false;

        private float groundClickTime = 0;

        private bool isGameObjectActive = false;

        private SceneBoundariesChecker boundariesChecker;
        private ParcelScene currentScene;

        private void Awake()
        {
            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
            SelectionParentCreate();
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
                DCLBuilderInput.OnMouseUp += OnMouseUp;
                DCLBuilderBridge.OnResetObject += OnResetObject;
                DCLBuilderBridge.OnEntityAdded += OnEntityAdded;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
                DCLBuilderBridge.OnBuilderSelectEntity += OnBuilderSelectEntity;
                DCLBuilderBridge.OnBuilderDeselectEntity += OnBuilderDeselectEntity;
                DCLBuilderGizmoManager.OnGizmoTransformObject += OnGizmoTransform;
                DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformEnded;
                DCLBuilderObjectDragger.OnDraggingObject += OnObjectsDrag;
                DCLBuilderObjectDragger.OnDraggingObjectEnd += OnObjectsDragEnd;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderInput.OnMouseDown -= OnMouseDown;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnResetObject -= OnResetObject;
            DCLBuilderBridge.OnEntityAdded -= OnEntityAdded;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
            DCLBuilderBridge.OnBuilderSelectEntity -= OnBuilderSelectEntity;
            DCLBuilderBridge.OnBuilderDeselectEntity -= OnBuilderDeselectEntity;
            DCLBuilderGizmoManager.OnGizmoTransformObject -= OnGizmoTransform;
            DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmoTransformEnded;
            DCLBuilderObjectDragger.OnDraggingObject -= OnObjectsDrag;
            DCLBuilderObjectDragger.OnDraggingObjectEnd -= OnObjectsDragEnd;
        }

        private void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                SelectionParentReset();
                OnSelectedObjectListChanged?.Invoke(selectedEntitiesParent, selectedEntities);
                if (selectedEntities.Count == 0)
                {
                    OnNoObjectSelected?.Invoke();
                }
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
                        OnGizmosAxisPressed?.Invoke(gizmosAxis);
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
                                    ProcessEntityPressed(pressedEntity, hit.point);
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
                if (entityEnqueueForDeselectInfo.pressedEntity != null)
                {
                    if ((Time.unscaledTime - entityEnqueueForDeselectInfo.pressedTime) < MAX_SECS_FOR_CLICK)
                    {
                        ProcessEntityPressed(entityEnqueueForDeselectInfo.pressedEntity, entityEnqueueForDeselectInfo.hitPoint);
                    }
                }
                entityEnqueueForDeselectInfo.pressedEntity = null;

                if (groundClickTime != 0 && (Time.unscaledTime - groundClickTime) < MAX_SECS_FOR_CLICK)
                {
                    DeselectAll();
                    if (selectedEntities != null)
                    {
                        OnNoObjectSelected?.Invoke();
                    }
                }
                groundClickTime = 0;
            }
        }

        private void OnResetObject()
        {
            for (int i = 0; i < selectedEntities.Count; i++)
            {
                selectedEntities[i].transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityAdded(DCLBuilderEntity entity)
        {
            if (!entities.ContainsKey(entity.rootEntity.entityId))
            {
                entities.Add(entity.rootEntity.entityId, entity);
            }
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (selectedEntities.Contains(entity))
            {
                Deselect(entity);
            }
            if (entities.ContainsKey(entity.rootEntity.entityId))
            {
                entities.Remove(entity.rootEntity.entityId);
            }
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            boundariesChecker = scene.boundariesChecker;
            currentScene = scene;
        }

        private void OnBuilderSelectEntity(string[] entitiesId)
        {
            List<DCLBuilderEntity> entitiesToDeselect = new List<DCLBuilderEntity>(selectedEntities);

            for (int i = 0; i < entitiesId.Length; i++)
            {
                if (entities.ContainsKey(entitiesId[i]))
                {
                    DCLBuilderEntity entity = entities[entitiesId[i]];
                    if (!SelectionParentHasChild(entity.transform))
                    {
                        Select(entity);
                    }
                    else
                    {
                        entitiesToDeselect.Remove(entity);
                    }
                }
            }

            for (int i = 0; i < entitiesToDeselect.Count; i++)
            {
                Deselect(entitiesToDeselect[i]);
            }
        }

        private void OnBuilderDeselectEntity(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
            {
                DeselectAll();
            }
            else
            {
                if (entities.ContainsKey(entityId))
                {
                    Deselect(entities[entityId]);
                }
            }
        }

        private void OnGizmoTransform(string gizmoType)
        {
            isSelectionTransformed = true;
        }

        private void OnGizmoTransformEnded(string gizmoType)
        {
            if (isSelectionTransformed)
            {
                Debug.Log("SelectionParentReset OnGizmoTransformEnded");
                SelectionParentReset();
            }
            isSelectionTransformed = false;
        }

        private void OnObjectsDrag()
        {
            isSelectionTransformed = true;
        }

        private void OnObjectsDragEnd()
        {
            if (isSelectionTransformed)
            {
                Debug.Log("SelectionParentReset OnObjectsDragEnd");
                SelectionParentReset();
            }
            isSelectionTransformed = false;
        }

        private bool CanSelect(DCLBuilderEntity entity)
        {
            return entity.hasGizmoComponent;
        }

        private void MarkSelected(DCLBuilderEntity entity)
        {
            if (entity != null)
            {
                OnMarkObjectSelected?.Invoke(entity, gizmosManager.GetSelectedGizmo());
            }
        }

        private void MarkDeselected(DCLBuilderEntity entity)
        {
            if (entity != null)
            {
                OnMarkObjectDeselected?.Invoke(entity, gizmosManager.GetSelectedGizmo());
            }
        }

        private void Select(DCLBuilderEntity entity)
        {
            if (entity != null)
            {
                if (!selectedEntities.Contains(entity))
                {
                    selectedEntities.Add(entity);
                }
                SelectionParentAddEntity(entity);
                entity.SetSelectLayer();

                OnSelectedObject?.Invoke(entity, gizmosManager.GetSelectedGizmo());
                isDirty = true;
            }
        }

        private void Deselect(DCLBuilderEntity entity)
        {
            if (entity != null)
            {
                SelectionParentRemoveEntity(entity);
                OnDeselectedObject?.Invoke(entity);
                entity.SetDefaultLayer();
            }
            if (selectedEntities.Contains(entity))
            {
                selectedEntities.Remove(entity);
            }
            isDirty = true;
        }

        private void DeselectAll()
        {
            for (int i = selectedEntities.Count - 1; i >= 0; i--)
            {
                Deselect(selectedEntities[i]);
            }
            SelectionParentRemoveAllEntities();
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

        private void ProcessEntityPressed(DCLBuilderEntity pressedEntity, Vector3 hitPoint)
        {
            if (CanSelect(pressedEntity))
            {
                MarkSelected(pressedEntity);
            }
        }

        private void SelectionParentCreate()
        {
            selectedEntitiesParent = new GameObject("BuilderSelectedEntitiesParent").GetComponent<Transform>();
        }

        private void SelectionParentReset()
        {
            if (selectedEntitiesParent.childCount == 0)
            {
                return;
            }

            Transform entitiyTransform = selectedEntitiesParent.GetChild(0);
            Vector3 min = entitiyTransform.position;
            Vector3 max = entitiyTransform.position;

            List<Transform> children = new List<Transform>(selectedEntitiesParent.childCount);

            children.Add(entitiyTransform);
            SelectionParentRemoveEntity(entitiyTransform);

            for (int i = selectedEntitiesParent.childCount - 1; i >= 0; i--)
            {
                entitiyTransform = selectedEntitiesParent.GetChild(i);
                if (entitiyTransform.position.x < min.x) min.x = entitiyTransform.position.x;
                if (entitiyTransform.position.y < min.y) min.y = entitiyTransform.position.y;
                if (entitiyTransform.position.z < min.z) min.z = entitiyTransform.position.z;
                if (entitiyTransform.position.x > max.x) max.x = entitiyTransform.position.x;
                if (entitiyTransform.position.y > max.y) max.y = entitiyTransform.position.y;
                if (entitiyTransform.position.z > max.z) max.z = entitiyTransform.position.z;

                children.Add(entitiyTransform);
                SelectionParentRemoveEntity(entitiyTransform);
            }

            selectedEntitiesParent.position = min + (max - min) * 0.5f;
            selectedEntitiesParent.localScale = Vector3.one;
            selectedEntitiesParent.rotation = Quaternion.identity;

            for (int i = 0; i < children.Count; i++)
            {
                SelectionParentAddEntity(children[i]);
            }
        }

        private void SelectionParentAddEntity(DCLBuilderEntity entity)
        {
            SelectionParentAddEntity(entity.transform);
        }

        private void SelectionParentAddEntity(Transform entityTransform)
        {
            entityTransform.SetParent(selectedEntitiesParent, true);
        }

        private void SelectionParentRemoveEntity(DCLBuilderEntity entity)
        {
            SelectionParentRemoveEntity(entity.transform);
        }

        private void SelectionParentRemoveEntity(Transform entityTransform)
        {
            entityTransform.SetParent(currentScene.transform, true);
        }

        private void SelectionParentRemoveAllEntities()
        {
            for (int i = selectedEntitiesParent.childCount - 1; i >= 0; i--)
            {
                SelectionParentRemoveEntity(selectedEntitiesParent.GetChild(i));
            }
        }

        private bool SelectionParentHasChild(Transform transform)
        {
            return transform.parent == selectedEntitiesParent;
        }

        private class EntityPressedInfo
        {
            public DCLBuilderEntity pressedEntity;
            public float pressedTime;
            public Vector3 hitPoint;
        }
    }
}