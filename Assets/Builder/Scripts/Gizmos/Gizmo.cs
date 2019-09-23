using UnityEngine;

namespace Builder
{
    public class Gizmo : MonoBehaviour
    {
        public string gizmoType;
        public bool transformWithObject;
        public GizmoAxis[] axes;

        public bool initialized { get; private set; }

        private Vector3 relativeScaleRatio;
        private bool isGameObjectActive = false;

        public void Initialize(Camera camera)
        {
            initialized = true;
            relativeScaleRatio = transform.localScale / GetCameraPlaneDistance(camera, transform.position);
        }

        public void SetObject(GameObject selectedObject)
        {
            if (selectedObject != null)
            {
                if (transformWithObject)
                {
                    transform.SetParent(selectedObject.transform);
                    transform.localPosition = Vector3.zero;

                }
                else
                {
                    transform.position = selectedObject.transform.position;
                }

                gameObject.SetActive(true);
            }
            else
            {
                transform.SetParent(null);
                gameObject.SetActive(false);
            }
        }
        public void SetSnapFactor(float position, float rotation, float scale)
        {
            for (int i = 0; i < axes.Length; i++)
            {
                axes[i].SetSnapFactor(position, rotation, scale);
            }
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderCamera.OnCameraZoomChanged += OnCameraZoomChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            DCLBuilderCamera.OnCameraZoomChanged -= OnCameraZoomChanged;
            isGameObjectActive = false;
        }

        private static float GetCameraPlaneDistance(Camera camera, Vector3 objectPosition)
        {
            Plane plane = new Plane(camera.transform.forward, camera.transform.position);
            return plane.GetDistanceToPoint(objectPosition);
        }

        private void OnCameraZoomChanged(Camera camera, float zoom)
        {
            float dist = GetCameraPlaneDistance(camera, transform.position);
            transform.localScale = relativeScaleRatio * dist;
        }
    }
}