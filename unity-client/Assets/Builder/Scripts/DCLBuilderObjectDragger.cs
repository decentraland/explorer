using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Builder
{
    public class DCLBuilderObjectDragger : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;

        public delegate void DragDelegate(DCLBuilderEntity entity, Vector3 position);

        public static event DragDelegate OnDraggingObjectStart;
        public static event DragDelegate OnDraggingObject;
        public static event DragDelegate OnDraggingObjectEnd;

        private List<SelectedEntityInfo> selectedEntitiesInfo = new List<SelectedEntityInfo>();
        private DCLBuilderEntity targetEntity;
        private Vector3 targetOffset;

        private float snapFactorPosition = 0;

        private bool isGameObjectActive = false;

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderObjectSelector.OnSelectedObjectListChanged += OnSelectedObjectListChanged;
                DCLBuilderObjectSelector.OnEntityPressed += OnSelectedObjectPressed;
                DCLBuilderInput.OnMouseDrag += OnMouseDrag;
                DCLBuilderInput.OnMouseUp += OnMouseUp;
                DCLBuilderBridge.OnSetGridResolution += OnSetGridResolution;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectSelector.OnSelectedObjectListChanged -= OnSelectedObjectListChanged;
            DCLBuilderObjectSelector.OnEntityPressed -= OnSelectedObjectPressed;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
        }

        private void OnSelectedObjectListChanged(List<DCLBuilderEntity> selectedEntities)
        {
            Vector3 defaultOffset = Vector3.zero;
            selectedEntitiesInfo.Clear();
            for (int i = 0; i < selectedEntities.Count; i++)
            {
                selectedEntitiesInfo.Add(new SelectedEntityInfo() { entity = selectedEntities[i], offset = defaultOffset });
            }
        }

        private void OnSelectedObjectPressed(DCLBuilderEntity entity, Vector3 hitPoint)
        {
            for (int i = 0; i < selectedEntitiesInfo.Count; i++)
            {
                selectedEntitiesInfo[i].offset = selectedEntitiesInfo[i].entity.transform.position - entity.transform.position;
                OnDraggingObjectStart?.Invoke(selectedEntitiesInfo[i].entity, selectedEntitiesInfo[i].entity.transform.position);
            }

            targetEntity = entity;
            targetOffset = entity.transform.position - hitPoint;
            builderRaycast.SetEntityHitPlane(hitPoint.y);
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0)
            {
                if (targetEntity != null)
                {
                    for (int i = 0; i < selectedEntitiesInfo.Count; i++)
                    {
                        OnDraggingObjectEnd?.Invoke(selectedEntitiesInfo[i].entity, selectedEntitiesInfo[i].entity.transform.position);
                    }
                }
                targetEntity = null;
            }
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0)
            {
                bool hasMouseMoved = (axisX != 0 || axisY != 0);
                if (targetEntity != null && hasMouseMoved)
                {
                    DragTargetEntity(mousePosition);
                }
            }
        }

        private void DragTargetEntity(Vector3 mousePosition)
        {
            Vector3 hitPosition = builderRaycast.RaycastToEntityHitPlane(mousePosition);
            Vector3 targetPosition = hitPosition + targetOffset;
            targetPosition.y = targetEntity.transform.position.y;

            if (snapFactorPosition > 0)
            {
                targetPosition.x = targetPosition.x - (targetPosition.x % snapFactorPosition);
                targetPosition.z = targetPosition.z - (targetPosition.z % snapFactorPosition);
            }

            for (int i = 0; i < selectedEntitiesInfo.Count; i++)
            {
                Vector3 entityNewPosition = targetPosition + selectedEntitiesInfo[i].offset;
                selectedEntitiesInfo[i].entity.transform.position = entityNewPosition;
                OnDraggingObject?.Invoke(selectedEntitiesInfo[i].entity, selectedEntitiesInfo[i].entity.transform.position);
            }
        }

        private void OnSetGridResolution(float position, float rotation, float scale)
        {
            snapFactorPosition = position;
        }

        class SelectedEntityInfo
        {
            public DCLBuilderEntity entity;
            public Vector3 offset;
        }
    }
}