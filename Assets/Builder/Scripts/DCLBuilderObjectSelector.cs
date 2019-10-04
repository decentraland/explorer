using UnityEngine;
using DCL.Controllers;
using Builder.Gizmos;

namespace Builder
{
    public class DCLBuilderObjectSelector : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;
        public DCLBuilderGizmoManager gizmosManager;

        public delegate void DragDelegate(DCLBuilderEntity entity, Vector3 position);
        public delegate void EntitySelectedDelegate(DCLBuilderEntity entity, string gizmoType);
        public delegate void EntityDeselectedDelegate(DCLBuilderEntity entity);

        public static event EntitySelectedDelegate OnSelectedObject;
        public static event EntityDeselectedDelegate OnDeselectedObject;
        public static event DragDelegate OnDraggingObjectStart;
        public static event DragDelegate OnDraggingObject;
        public static event DragDelegate OnDraggingObjectEnd;

        private DCLBuilderEntity selectedEntity = null;

        private DragInfo dragInfo = new DragInfo();

        private float snapFactorPosition = 0;

        private bool isGameObjectActive = false;

        private SceneBoundariesChecker boundariesChecker;

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
                DCLBuilderBridge.OnEntityAdded += OnEntityAdded;
                DCLBuilderBridge.OnEntityRemoved += OnEntityRemoved;
                DCLBuilderBridge.OnSetGridResolution += OnSetGridResolution;
                DCLBuilderBridge.OnSceneChanged += OnSceneChanged;
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
            DCLBuilderBridge.OnEntityAdded -= OnEntityAdded;
            DCLBuilderBridge.OnEntityRemoved -= OnEntityRemoved;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
            DCLBuilderBridge.OnSceneChanged -= OnSceneChanged;
        }

        private void Update()
        {
            if (!gizmosManager.isTransformingObject)
            {
                CheckGizmoHover(Input.mousePosition);
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
                        gizmosManager.OnBeginDrag(gizmosAxis, selectedEntity);
                        builderRaycast.SetGizmoHitPlane(gizmosAxis);
                    }
                    else
                    {
                        dragInfo.entity = hit.collider.gameObject.GetComponent<DCLBuilderEntity>();
                        if (dragInfo.entity != null)
                        {
                            Debug.Log("unity-client: pressed entity: " + dragInfo.entity.rootEntity.entityId);
                            if (CanSelect(dragInfo.entity))
                            {
                                Debug.Log("unity-client: can selected entity: " + dragInfo.entity.rootEntity.entityId);
                                if (dragInfo.entity != selectedEntity)
                                {
                                    Debug.Log("unity-client: select entity: " + dragInfo.entity.rootEntity.entityId);
                                    Select(dragInfo.entity);
                                }

                                dragInfo.isDraggingObject = true;
                                builderRaycast.SetEntityHitPlane(hit.point.y);
                                dragInfo.hitToEntityOffset = dragInfo.entity.transform.position - hit.point;
                                OnDraggingObjectStart?.Invoke(dragInfo.entity, dragInfo.entity.transform.position);
                            }
                        }
                    }
                }
            }
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0)
            {
                if (dragInfo.isDraggingObject && dragInfo.entity != null)
                {
                    Debug.Log("unity-client: drag entity ends: " + dragInfo.entity.rootEntity.entityId);
                    OnDraggingObjectEnd?.Invoke(dragInfo.entity, dragInfo.entity.transform.position);
                }

                Debug.Log("unity-client: stop drag entity");
                dragInfo.isDraggingObject = false;
                dragInfo.entity = null;

                if (gizmosManager.isTransformingObject)
                {
                    gizmosManager.OnEndDrag();
                }
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
                else if (dragInfo.isDraggingObject && dragInfo.entity != null && hasMouseMoved)
                {
                    DragObject(dragInfo.entity, mousePosition);
                }
            }
        }

        private void OnSetGridResolution(float position, float rotation, float scale)
        {
            snapFactorPosition = position;
        }


        private void OnResetObject()
        {
            if (selectedEntity != null)
            {
                selectedEntity.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEntityRemoved(DCLBuilderEntity entity)
        {
            if (selectedEntity == entity)
            {
                Deselect();
                dragInfo.isDraggingObject = false;
                dragInfo.entity = null;
            }
        }

        private void OnEntityAdded(DCLBuilderEntity entity)
        {
            if (!dragInfo.isDraggingObject && !gizmosManager.isTransformingObject && CanSelect(entity))
            {
                Debug.Log("unity-client: autoselect entity: " + entity.rootEntity.entityId);
                Select(entity);
            }
        }

        private void OnSceneChanged(ParcelScene scene)
        {
            boundariesChecker = scene.boundariesChecker;
        }

        private bool CanSelect(DCLBuilderEntity entity)
        {
            return entity.hasGizmoComponent;
        }

        private void Select(DCLBuilderEntity entity)
        {
            Deselect();
            if (entity != null)
            {
                Debug.Log("unity-client: do select entity: " + entity.rootEntity.entityId);
                selectedEntity = entity;
                selectedEntity.Select();

                OnSelectedObject?.Invoke(entity, gizmosManager.GetSelectedGizmo());
            }
        }

        private void Deselect(DCLBuilderEntity entity)
        {
            if (selectedEntity == entity)
            {
                OnDeselectedObject?.Invoke(entity);
                if (entity != null)
                {
                    Debug.Log("unity-client: do deselect entity: " + entity.rootEntity.entityId);
                    entity.Deselect();
                }
                selectedEntity = null;
            }
        }

        private void Deselect()
        {
            if (selectedEntity != null)
            {
                Deselect(selectedEntity);
            }
        }

        private void DragObject(DCLBuilderEntity entity, Vector3 mousePosition)
        {
            Vector3 hitPosition = builderRaycast.RaycastToEntityHitPlane(mousePosition);
            Vector3 newPosition = hitPosition + dragInfo.hitToEntityOffset;
            newPosition.y = entity.transform.position.y;

            if (snapFactorPosition > 0)
            {
                newPosition.x = newPosition.x - (newPosition.x % snapFactorPosition);
                newPosition.z = newPosition.z - (newPosition.z % snapFactorPosition);
            }

            entity.transform.position = newPosition;
            boundariesChecker?.EvaluateEntityPosition(selectedEntity.rootEntity);
            Debug.Log("unity-client: drag entity: " + entity.rootEntity.entityId);

            OnDraggingObject?.Invoke(entity, newPosition);
        }

        private void UpdateGizmoAxis(Vector3 mousePosition)
        {
            Vector3 hit;
            if (builderRaycast.RaycastToGizmosHitPlane(mousePosition, out hit))
            {
                gizmosManager.OnDrag(hit);
                boundariesChecker?.EvaluateEntityPosition(selectedEntity.rootEntity);
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

        // TODO: remove?
        private void SelectionEffect(DCLBuilderEntity entity)
        {
            gameObject.layer = builderRaycast.selectionLayer;
            ChangeLayersRecursively(gameObject.transform, builderRaycast.selectionLayer, builderRaycast.defaultLayer);
        }

        // TODO: remove?
        private void UnSelectionEffect(GameObject gameObject)
        {
            gameObject.layer = builderRaycast.defaultLayer;
            ChangeLayersRecursively(gameObject.transform, builderRaycast.defaultLayer, builderRaycast.selectionLayer);
        }

        private void ChangeLayersRecursively(Transform root, int layer, int currentLayer)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.gameObject.layer == currentLayer)
                {
                    child.gameObject.layer = layer;
                    ChangeLayersRecursively(child, layer, currentLayer);
                }
            }
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            Deselect();
            gameObject.SetActive(!isPreview);
        }

        private class DragInfo
        {
            public DCLBuilderEntity entity = null;
            public bool isDraggingObject = false;
            public Vector3 hitToEntityOffset = Vector3.zero;
        }
    }
}