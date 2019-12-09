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

        private List<DCLBuilderEntity> selectedEntities;
        private Vector3 targetOffset;
        private Transform selectedEntitiesParent;
        private bool isDragging = false;


        private float snapFactorPosition = 0;

        private bool isGameObjectActive = false;

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderObjectSelector.OnSelectedObjectListChanged += OnSelectedObjectListChanged;
                DCLBuilderObjectSelector.OnEntityPressed += OnEntityPressed;
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
            DCLBuilderObjectSelector.OnEntityPressed -= OnEntityPressed;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
        }

        private void OnSelectedObjectListChanged(Transform selectionParent, List<DCLBuilderEntity> selectedEntities)
        {
            this.selectedEntitiesParent = selectionParent;
            this.selectedEntities = selectedEntities;
        }

        private void OnEntityPressed(DCLBuilderEntity entity, Vector3 hitPoint)
        {
            if (selectedEntities != null)
            {
                if (OnDraggingObjectStart != null)
                {
                    for (int i = 0; i < selectedEntities.Count; i++)
                    {
                        OnDraggingObjectStart.Invoke(selectedEntities[i], selectedEntities[i].transform.position);
                    }
                }

                targetOffset = selectedEntitiesParent.position - hitPoint;
                builderRaycast.SetEntityHitPlane(hitPoint.y);
                isDragging = true;
            }
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            if (buttonId == 0 && selectedEntities != null)
            {
                if (isDragging && OnDraggingObjectEnd != null)
                {
                    for (int i = 0; i < selectedEntities.Count; i++)
                    {
                        OnDraggingObjectEnd.Invoke(selectedEntities[i], selectedEntities[i].transform.position);
                    }
                }
                isDragging = false;
            }
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            if (buttonId == 0 && selectedEntities != null)
            {
                bool hasMouseMoved = (axisX != 0 || axisY != 0);
                if (isDragging && hasMouseMoved)
                {
                    DragTargetEntity(mousePosition);
                }
            }
        }

        private void DragTargetEntity(Vector3 mousePosition)
        {
            Vector3 hitPosition = builderRaycast.RaycastToEntityHitPlane(mousePosition);
            Vector3 targetPosition = hitPosition + targetOffset;
            targetPosition.y = selectedEntitiesParent.position.y;

            if (snapFactorPosition > 0)
            {
                targetPosition.x = targetPosition.x - (targetPosition.x % snapFactorPosition);
                targetPosition.z = targetPosition.z - (targetPosition.z % snapFactorPosition);
            }

            Vector3 moveAmount = targetPosition - selectedEntitiesParent.transform.position;
            selectedEntitiesParent.transform.position = targetPosition;

            if (OnDraggingObject != null)
            {
                for (int i = 0; i < selectedEntities.Count; i++)
                {
                    OnDraggingObject.Invoke(selectedEntities[i], selectedEntities[i].transform.position);
                }
            }
        }

        private void OnSetGridResolution(float position, float rotation, float scale)
        {
            snapFactorPosition = position;
        }
    }
}