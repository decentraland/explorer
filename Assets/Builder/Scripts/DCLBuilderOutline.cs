using UnityEngine;
using UnityEngine.UI;

namespace Builder
{
    [RequireComponent(typeof(Camera))]
    public class DCLBuilderOutline : MonoBehaviour
    {
        [SerializeField] Material OutlineMaterial = null;

        private Camera builderCamera;

        private Camera outlineCamera;
        private Canvas outlineCanvas;
        private RawImage outlineRawImage;

        private RenderTexture outlinedRenderTexture;

        private int lastScreenWidth = 0;
        private int lastScreenHeight = 0;

        private void Awake()
        {
            builderCamera = GetComponent<Camera>();

            outlineCamera = new GameObject("BuilderOutlineCamera").AddComponent<Camera>();
            outlineCamera.CopyFrom(builderCamera);
            outlineCamera.clearFlags = CameraClearFlags.SolidColor;
            outlineCamera.backgroundColor = Color.clear;
            outlineCamera.cullingMask = LayerMask.GetMask(DCLBuilderRaycast.LAYER_SELECTION);
            outlineCamera.depth = builderCamera.depth - 1;
            outlineCamera.transform.SetParent(builderCamera.transform);

            outlineCanvas = new GameObject("BuilderOutlineCanvas").AddComponent<Canvas>();
            outlineCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            outlineCanvas.worldCamera = builderCamera;
            outlineCanvas.planeDistance = 1;

            outlineRawImage = new GameObject("BuilderOutlineRawImage").AddComponent<RawImage>();
            outlineRawImage.transform.SetParent(outlineCanvas.transform);
            outlineRawImage.transform.localPosition = Vector3.zero;
            outlineRawImage.transform.localRotation = Quaternion.identity;
            outlineRawImage.transform.localScale = Vector3.one;

            outlineRawImage.rectTransform.sizeDelta = new Vector2(outlineCanvas.pixelRect.width, outlineCanvas.pixelRect.height);
            outlineRawImage.raycastTarget = false;
            outlineRawImage.material = OutlineMaterial;

            DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
        }

        private void OnDestroy()
        {
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void OnResize()
        {
            outlinedRenderTexture = new RenderTexture(builderCamera.pixelWidth, builderCamera.pixelHeight, 24);
            outlineCamera.targetTexture = outlinedRenderTexture;
            outlineRawImage.material.mainTexture = outlinedRenderTexture;
            outlineRawImage.rectTransform.sizeDelta = new Vector2(builderCamera.pixelWidth, builderCamera.pixelHeight);
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            outlineCanvas.gameObject.SetActive(!isPreview);
        }

        private void Update()
        {
            if ((Screen.width > 0 && Screen.height > 0) && (lastScreenWidth != Screen.width || lastScreenHeight != Screen.height))
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                OnResize();
            }
        }
    }
}
