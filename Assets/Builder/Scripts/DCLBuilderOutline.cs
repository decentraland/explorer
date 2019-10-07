using UnityEngine;
using UnityEngine.UI;

namespace Builder
{
    [RequireComponent(typeof(Camera))]
    public class DCLBuilderOutline : MonoBehaviour
    {
        [SerializeField] Material PostOutlineMaterial = null;
        [SerializeField] Material GaussianBlurMaterial = null;

        private Camera builderCamera;

        private Camera outlineCamera;
        private Canvas outlineCanvas;
        private RawImage outlineImage;

        private RenderTexture inputRenderTexture;
        private RenderTexture outlinedRenderTexture;
        private RenderTexture outputRenderTexture;

        private int lastScreenWidth = 0;
        private int lastScreenHeight = 0;

        private bool isGameObjectActive = false;

        private bool isEntitySelected = false;
        private bool isGizmoTransformingEntity = false;
        private bool isDraggingEntity = false;

        private void Awake()
        {
            builderCamera = GetComponent<Camera>();

            outlineCamera = new GameObject("BuilderOutlineCamera").AddComponent<Camera>();
            outlineCamera.CopyFrom(builderCamera);
            outlineCamera.clearFlags = CameraClearFlags.Nothing;
            outlineCamera.cullingMask = LayerMask.GetMask(DCLBuilderRaycast.LAYER_SELECTION);
            outlineCamera.depth = builderCamera.depth - 1;
            outlineCamera.transform.SetParent(builderCamera.transform);

            outlineCanvas = new GameObject("BuilderOutlineCanvas").AddComponent<Canvas>();
            outlineCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            outlineCanvas.worldCamera = builderCamera;
            outlineCanvas.planeDistance = 1;

            outlineImage = new GameObject("BuilderOutlineRawImage").AddComponent<RawImage>();
            outlineImage.transform.SetParent(outlineCanvas.transform);
            outlineImage.transform.localPosition = Vector3.zero;
            outlineImage.transform.localRotation = Quaternion.identity;
            outlineImage.transform.localScale = Vector3.one;

            outlineImage.rectTransform.sizeDelta = new Vector2(outlineCanvas.pixelRect.width, outlineCanvas.pixelRect.height);
            outlineImage.raycastTarget = false;

            OnResize();

            outlineCanvas.gameObject.SetActive(false);

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
                DCLBuilderObjectSelector.OnSelectedObject += OnEntitySelected;
                DCLBuilderObjectSelector.OnDeselectedObject += OnEntityDeselected;
                Gizmos.DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmoTransformObjectStart;
                Gizmos.DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmoTransformObjectEnd;
                DCLBuilderObjectSelector.OnDraggingObjectStart += OnEntityDragStart;
                DCLBuilderObjectSelector.OnDraggingObjectEnd += OnEntityDragEnd;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectSelector.OnSelectedObject -= OnEntitySelected;
            DCLBuilderObjectSelector.OnDeselectedObject -= OnEntityDeselected;
            Gizmos.DCLBuilderGizmoManager.OnGizmoTransformObjectStart -= OnGizmoTransformObjectStart;
            Gizmos.DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmoTransformObjectEnd;
            DCLBuilderObjectSelector.OnDraggingObjectStart -= OnEntityDragStart;
            DCLBuilderObjectSelector.OnDraggingObjectEnd -= OnEntityDragEnd;
        }

        private void OnResize()
        {
            inputRenderTexture = new RenderTexture(builderCamera.pixelWidth, builderCamera.pixelHeight, 24);
            outlineCamera.targetTexture = inputRenderTexture;
            outlinedRenderTexture = new RenderTexture(inputRenderTexture.width, inputRenderTexture.height, inputRenderTexture.depth);
            outputRenderTexture = new RenderTexture(inputRenderTexture.width, inputRenderTexture.height, inputRenderTexture.depth);
            outlineImage.texture = outputRenderTexture;
        }

        private void ProcessOutline()
        {
            if (isEntitySelected)
            {
                RenderTexture rt = UnityEngine.RenderTexture.active;
                UnityEngine.RenderTexture.active = outputRenderTexture;
                GL.Clear(true, true, Color.clear);
                UnityEngine.RenderTexture.active = outlinedRenderTexture;
                GL.Clear(true, true, Color.clear);
                UnityEngine.RenderTexture.active = rt;

                Graphics.Blit(inputRenderTexture, outlinedRenderTexture, PostOutlineMaterial);
                Graphics.Blit(outlinedRenderTexture, outputRenderTexture, GaussianBlurMaterial);
                outlineImage.texture = outputRenderTexture;
                outlineImage.rectTransform.sizeDelta = new Vector2(builderCamera.pixelWidth, builderCamera.pixelHeight);
            }
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            outlineCanvas.gameObject.SetActive(!isPreview);
        }

        private void Update()
        {
            if (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                OnResize();
                ProcessOutline();
            }
            if (ShouldProcessOutline())
            {
                ProcessOutline();
            }
        }

        private bool ShouldProcessOutline()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                return true;
            }
            return isGizmoTransformingEntity || isDraggingEntity;
        }

        private void OnEntitySelected(DCLBuilderEntity entity, string gizmoType)
        {
            isEntitySelected = entity != null;
            outlineCanvas.gameObject.SetActive(true);
        }

        private void OnEntityDeselected(DCLBuilderEntity entity)
        {
            isEntitySelected = false;
        }

        private void OnGizmoTransformObjectStart(DCLBuilderEntity entity, Vector3 position, string gizmoType)
        {
            isGizmoTransformingEntity = true;
        }

        private void OnGizmoTransformObjectEnd(DCLBuilderEntity entity, Vector3 position, string gizmoType)
        {
            isGizmoTransformingEntity = false;
        }

        private void OnEntityDragStart(DCLBuilderEntity entity, Vector3 position)
        {
            isDraggingEntity = true;
        }

        private void OnEntityDragEnd(DCLBuilderEntity entity, Vector3 position)
        {
            isDraggingEntity = false;
        }
    }
}
