using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderScaleGizmo : DCLBuilderGizmo
    {
        const float MINIMUN_SCALE_ALLOWED = 0.01f;

        [SerializeField] DCLBuilderGizmoAxis axisProportionalScale = null;

        public override void Initialize(Camera camera)
        {
            base.Initialize(camera);
            axisProportionalScale.SetGizmo(this);
        }

        public override void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo)
        {
            snapFactor = snapInfo.scale;
        }

        public override void TransformEntity(Transform entityTransform, DCLBuilderGizmoAxis axis, float axisValue)
        {
            Vector3 scaleDirection = activeAxis.transform.forward;
            if (axis == axisProportionalScale)
            {
                scaleDirection = Vector3.one;
            }
            else if (worldOrientedGizmos)
            {
                scaleDirection = entityTransform.rotation * activeAxis.transform.forward;
            }

            Vector3 newScale = entityTransform.localScale + scaleDirection * axisValue;

            if (Mathf.Abs(newScale.x) < MINIMUN_SCALE_ALLOWED || Mathf.Abs(newScale.y) < MINIMUN_SCALE_ALLOWED || Mathf.Abs(newScale.y) < MINIMUN_SCALE_ALLOWED)
            {
                newScale += scaleDirection * MINIMUN_SCALE_ALLOWED;
            }

            entityTransform.localScale = newScale;
        }

        protected override void SetPreviousAxisValue(float axisValue, float transformValue)
        {
            prevAxisValue = axisValue;
        }

        protected override float GetHitPointToAxisValue(DCLBuilderGizmoAxis axis, Vector3 hitPoint)
        {
            if (axis == axisProportionalScale)
            {
                return Vector3.Distance(transform.position, hitPoint);
            }
            return axis.transform.InverseTransformPoint(hitPoint).z;
        }
    }
}