using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DCL
{
    public class MapRenderer : MonoBehaviour
    {
        const int LEFT_BORDER_PARCELS = 25;
        const int RIGHT_BORDER_PARCELS = 31;
        const int TOP_BORDER_PARCELS = 31;
        const int BOTTOM_BORDER_PARCELS = 25;
        const int WORLDMAP_WIDTH_IN_PARCELS = 300;
        private int NAVMAP_CHUNK_LAYER;

        public static MapRenderer i { get; private set; }

        [SerializeField] private float parcelHightlightScale = 1.25f;
        [SerializeField] private float parcelHoldTimeInSeconds = 1f;
        private float parcelSizeInMap;
        private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;
        private Vector3[] mapWorldspaceCorners = new Vector3[4];
        private Vector3 worldCoordsOriginInMap;
        private Vector3 lastMouseMapCoords;
        private float parcelHoldCounter;
        private List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
        private PointerEventData uiRaycastPointerEventData = new PointerEventData(EventSystem.current);

        [HideInInspector] public Vector3 mouseMapCoords;
        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());
        public MapAtlas atlas;
        public RawImage parcelHighlightImage;
        public TextMeshProUGUI highlightedParcelText;
        public Transform overlayContainer;

        public Image playerPositionIcon;

        // Used as a reference of the coordinates origin in-map and as a parcel width/height reference
        public RectTransform centeredReferenceParcel;

        public MapSceneIcon scenesOfInterestIconPrefab;

        private HashSet<MinimapMetadata.MinimapSceneInfo> scenesOfInterest = new HashSet<MinimapMetadata.MinimapSceneInfo>();
        private Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject> scenesOfInterestMarkers = new Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject>();

        private bool parcelHighlightEnabledValue = false;
        public bool parcelHighlightEnabled
        {
            set
            {
                parcelHighlightEnabledValue = value;
                parcelHighlightImage.gameObject.SetActive(parcelHighlightEnabledValue);
            }
            get
            {
                return parcelHighlightEnabledValue;
            }
        }

        public static System.Action<int, int> OnParcelClicked;
        public static System.Action<int, int> OnParcelHold;
        public static System.Action OnParcelHoldCancel;

        private void Awake()
        {
            i = this;

            NAVMAP_CHUNK_LAYER = LayerMask.NameToLayer("NavmapChunk");

            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += MapRenderer_OnSceneInfoUpdated;

            playerWorldPosition.OnChange += OnCharacterMove;
            playerRotation.OnChange += OnCharacterRotate;

            parcelHighlightImage.rectTransform.localScale = new Vector3(parcelHightlightScale, parcelHightlightScale, 1f);
        }

        void Update()
        {
            if (!parcelHighlightEnabled) return;

            parcelSizeInMap = centeredReferenceParcel.rect.width * centeredReferenceParcel.lossyScale.x;

            // the reference parcel has a bottom-left pivot
            centeredReferenceParcel.GetWorldCorners(mapWorldspaceCorners);
            worldCoordsOriginInMap = mapWorldspaceCorners[0];

            UpdateMouseMapCoords();

            UpdateParcelHighlight();

            UpdateParcelHold();

            lastMouseMapCoords = mouseMapCoords;
        }

        void UpdateMouseMapCoords()
        {
            if (!IsCursorOverMapChunk()) return;

            mouseMapCoords = Input.mousePosition - worldCoordsOriginInMap;
            mouseMapCoords = mouseMapCoords / parcelSizeInMap;

            mouseMapCoords.x = (int)Mathf.Floor(mouseMapCoords.x);
            mouseMapCoords.y = (int)Mathf.Floor(mouseMapCoords.y);
        }

        bool IsCursorOverMapChunk()
        {
            uiRaycastPointerEventData.position = Input.mousePosition;
            EventSystem.current.RaycastAll(uiRaycastPointerEventData, uiRaycastResults);

            return uiRaycastResults.Count > 0 && uiRaycastResults[0].gameObject.layer == NAVMAP_CHUNK_LAYER;
        }

        void UpdateParcelHighlight()
        {
            if (!CoordinatesAreInsideTheWorld((int)mouseMapCoords.x, (int)mouseMapCoords.y))
            {
                if (parcelHighlightImage.gameObject.activeSelf)
                    parcelHighlightImage.gameObject.SetActive(false);

                return;
            }

            if (!parcelHighlightImage.gameObject.activeSelf)
                parcelHighlightImage.gameObject.SetActive(true);

            parcelHighlightImage.transform.position = worldCoordsOriginInMap + mouseMapCoords * parcelSizeInMap + new Vector3(parcelSizeInMap, parcelSizeInMap, 0f) / 2;
            highlightedParcelText.text = $"{mouseMapCoords.x}, {mouseMapCoords.y}";

            // ----------------------------------------------------
            // TODO: Use sceneInfo to highlight whole scene parcels and populate scenes hover info on navmap once we can access all the scenes info
            // var sceneInfo = mapMetadata.GetSceneInfo(mouseMapCoords.x, mouseMapCoords.y);
        }

        void UpdateParcelHold()
        {
            if (mouseMapCoords == lastMouseMapCoords)
            {
                parcelHoldCounter += Time.deltaTime;

                if (parcelHoldCounter >= parcelHoldTimeInSeconds)
                {
                    parcelHoldCounter = 0f;
                    OnParcelHold?.Invoke((int)mouseMapCoords.x, (int)mouseMapCoords.y);
                }
            }
            else
            {
                parcelHoldCounter = 0f;
                OnParcelHoldCancel?.Invoke();
            }
        }

        bool CoordinatesAreInsideTheWorld(int xCoord, int yCoord)
        {
            return (Mathf.Abs(xCoord) <= WORLDMAP_WIDTH_IN_PARCELS / 2) && (Mathf.Abs(yCoord) <= WORLDMAP_WIDTH_IN_PARCELS / 2);
        }

        private void MapRenderer_OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (!sceneInfo.isPOI)
                return;

            if (scenesOfInterest.Contains(sceneInfo))
                return;

            scenesOfInterest.Add(sceneInfo);

            GameObject go = Object.Instantiate(scenesOfInterestIconPrefab.gameObject, overlayContainer.transform);

            Vector2 centerTile = Vector2.zero;

            foreach (var parcel in sceneInfo.parcels)
            {
                centerTile += parcel;
            }

            centerTile /= (float)sceneInfo.parcels.Count;

            (go.transform as RectTransform).anchoredPosition = MapUtils.GetTileToLocalPosition(centerTile.x, centerTile.y);

            MapSceneIcon icon = go.GetComponent<MapSceneIcon>();
            icon.title.text = sceneInfo.name;

            scenesOfInterestMarkers.Add(sceneInfo, go);
        }

        public void OnDestroy()
        {
            playerWorldPosition.OnChange -= OnCharacterMove;
            playerRotation.OnChange -= OnCharacterRotate;
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= MapRenderer_OnSceneInfoUpdated;
        }

        private void OnCharacterMove(Vector3 current, Vector3 previous)
        {
            UpdateRendering(Utils.WorldToGridPositionUnclamped(current));
        }

        private void OnCharacterRotate(Vector3 current, Vector3 previous)
        {
            UpdateRendering(Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get()));
        }

        public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
        {
            UpdateRendering(new Vector2((float)newCoords.x, (float)newCoords.y));
        }

        public void UpdateRendering(Vector2 newCoords)
        {
            UpdateBackgroundLayer(newCoords);
            UpdateSelectionLayer();
            UpdateOverlayLayer();
        }

        void UpdateBackgroundLayer(Vector2 newCoords)
        {
            atlas.CenterToTile(newCoords);
        }

        void UpdateSelectionLayer()
        {
            //TODO(Brian): Build and place here the scene highlight if applicable.
        }

        void UpdateOverlayLayer()
        {
            //NOTE(Brian): Player icon
            Vector3 f = CommonScriptableObjects.cameraForward.Get();
            Quaternion playerAngle = Quaternion.Euler(0, 0, Mathf.Atan2(-f.x, f.z) * Mathf.Rad2Deg);

            var gridPosition = this.playerGridPosition;
            playerPositionIcon.transform.localPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
            playerPositionIcon.transform.rotation = playerAngle;
        }

        public Vector3 GetViewportCenter()
        {
            return atlas.viewport.TransformPoint(atlas.viewport.rect.center);
        }

        // Called by the parcelhighlight image button
        public void ClickMousePositionParcel()
        {
            OnParcelClicked?.Invoke((int)mouseMapCoords.x, (int)mouseMapCoords.y);
        }
    }
}
