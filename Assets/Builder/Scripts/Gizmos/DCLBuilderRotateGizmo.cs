using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderRotateGizmo : DCLBuilderGizmo
    {
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

        public override Vector3 GetPlaneNormal()
        {
            return activeAxis.transform.forward;
        }

        protected override float GetHitPointToAxisValue(DCLBuilderGizmoAxis axis, Vector3 hitPoint)
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