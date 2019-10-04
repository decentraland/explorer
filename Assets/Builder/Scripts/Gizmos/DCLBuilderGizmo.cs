using UnityEngine;

namespace Builder.Gizmos
{
    public abstract class DCLBuilderGizmo : MonoBehaviour
    {
        [SerializeField] private string gizmoType = string.Empty;
        [SerializeField] protected DCLBuilderGizmoAxis axisX;
        [SerializeField] protected DCLBuilderGizmoAxis axisY;
        [SerializeField] protected DCLBuilderGizmoAxis axisZ;

        public bool initialized { get; private set; }

        protected float snapFactor = 0;

        protected bool worldOrientedGizmos = true;
        private Transform targetTransform = null;

        protected Camera builderCamera;

        private Vector3 relativeScaleRatio;
        private bool startDragging = false;
        protected float prevAxisValue;

        protected DCLBuilderGizmoAxis activeAxis;

        public abstract void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo);
        public abstract void TransformEntity(Transform targetTransform, DCLBuilderGizmoAxis axis, float axisValue);

        public virtual void Initialize(Camera camera)
        {
            initialized = true;
            relativeScaleRatio = transform.localScale / GetCameraPlaneDistance(camera, transform.position);
            builderCamera = camera;
            axisX.SetGizmo(this);
            axisY.SetGizmo(this);
            axisZ.SetGizmo(this);
        }

        public string GetGizmoType()
        {
            return gizmoType;
        }

        public virtual void SetTargetTransform(Transform entityTransform)
        {
            targetTransform = entityTransform;
            SetPositionToTarget();
        }

        public virtual void OnBeginDrag(DCLBuilderGizmoAxis axis, Transform entityTransform)
        {
            startDragging = true;
            targetTransform = entityTransform;
            activeAxis = axis;
            axis.SetColorHighlight();
        }

        public virtual void OnDrag(Vector3 hitPoint)
        {
            float axisValue = GetHitPointToAxisValue(activeAxis, hitPoint);

            if (startDragging)
            {
                startDragging = false;
                prevAxisValue = axisValue;
            }

            float transformValue = axisValue - prevAxisValue;
            if (Mathf.Abs(transformValue) >= snapFactor)
            {
                if (snapFactor > 0)
                {
                    float sign = Mathf.Sign(transformValue);
                    transformValue = transformValue + (Mathf.Abs(transformValue) % snapFactor) * -sign;
                }

                SetPreviousAxisValue(axisValue, transformValue);
                TransformEntity(targetTransform, activeAxis, transformValue);
            }
        }

        public virtual void OnEndDrag()
        {
            activeAxis.SetColorDefault();
        }

        public virtual Vector3 GetPlaneNormal()
        {
            return activeAxis.transform.up;
        }

        protected virtual float GetHitPointToAxisValue(DCLBuilderGizmoAxis axis, Vector3 hitPoint)
        {
            return axis.transform.InverseTransformPoint(hitPoint).z;
        }

        protected virtual void SetPreviousAxisValue(float axisValue, float transformValue)
        {
            prevAxisValue = axisValue - transformValue;
        }

        private void SetPositionToTarget()
        {
            if (targetTransform)
            {
                transform.position = targetTransform.position;
                if (!worldOrientedGizmos)
                {
                    transform.rotation = targetTransform.rotation;
                }
            }
        }

        private void Update()
        {
            SetPositionToTarget();
            if (builderCamera)
            {
                float dist = GetCameraPlaneDistance(builderCamera, transform.position);
                transform.localScale = relativeScaleRatio * dist;
            }
        }

        private static float GetCameraPlaneDistance(Camera camera, Vector3 objectPosition)
        {
            Plane plane = new Plane(camera.transform.forward, camera.transform.position);
            return plane.GetDistanceToPoint(objectPosition);
        }
    }
}