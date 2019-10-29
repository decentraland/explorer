using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderScaleGizmo : DCLBuilderGizmo
    {
        const float MINIMUN_SCALE_ALLOWED = 0.01f;

        [SerializeField] DCLBuilderGizmoAxis axisProportionalScale = null;

        private Vector3 lastHitPoint;
        private Vector3 inputReferencePoint;

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
                float inputViewportDir = builderCamera.WorldToViewportPoint(lastHitPoint).y - builderCamera.WorldToViewportPoint(inputReferencePoint).y;
                if (inputViewportDir > 0)
                {
                    scaleDirection = -Vector3.one;
                }
                inputReferencePoint = lastHitPoint;
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
            prevAxisValue = 0;
        }

        protected override float GetHitPointToAxisValue(DCLBuilderGizmoAxis axis, Vector3 hitPoint)
        {
            if (axis == axisProportionalScale)
            {
                if (startDragging)
                {
                    inputReferencePoint = hitPoint;
                }
                lastHitPoint = hitPoint;
                return Vector3.Distance(inputReferencePoint, hitPoint);
            }
            return axis.transform.InverseTransformPoint(hitPoint).z;
        }
    }
}