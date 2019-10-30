﻿using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderRotateGizmo : DCLBuilderGizmo
    {
        private Plane raycastPlane;

        public override void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo)
        {
            snapFactor = snapInfo.rotation;
        }

        public override void TransformEntity(Transform entityTransform, DCLBuilderGizmoAxis axis, float axisValue)
        {
            Space space = worldOrientedGizmos ? Space.World : Space.Self;
            Vector3 rotationVector = activeAxis.transform.forward;
            entityTransform.Rotate(rotationVector, axisValue * Mathf.Rad2Deg, space);
        }

        public override void OnBeginDrag(DCLBuilderGizmoAxis axis, Transform entityTransform)
        {
            base.OnBeginDrag(axis, entityTransform);
            raycastPlane = new Plane(activeAxis.transform.forward, transform.position);
        }

        public override bool RaycastHit(Ray ray, out Vector3 hitPoint)
        {
            float enter = 0.0f;

            if (raycastPlane.Raycast(ray, out enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }
            hitPoint = Vector3.zero;
            return false;
        }

        protected override float GetHitPointToAxisValue(DCLBuilderGizmoAxis axis, Vector3 hitPoint, Vector2 mousePosition)
        {
            Vector3 hitDir = (hitPoint - transform.position).normalized;
            return Vector3.SignedAngle(axis.transform.up, hitDir, axis.transform.forward) * Mathf.Deg2Rad;
        }

        protected override void SetPreviousAxisValue(float axisValue, float transformValue)
        {
            prevAxisValue = axisValue;
        }
    }
}