using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DCL
{
    public class MapRenderer : MonoBehaviour
    {
        const int LEFT_BORDER_PARCELS = 25;
        const int BOTTOM_BORDER_PARCELS = 25;
        const int WORLDMAP_WIDTH_IN_PARCELS = 300;

        public static MapRenderer i { get; private set; }

        [SerializeField] private float parcelHightlightScale = 1.25f;
        private int parcelSizeInMap;
        private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;
        private Vector3[] mapWorldspaceCorners = new Vector3[4];
        private Vector3 worldmapOffset;
        private Vector3 worldCoordsOriginInMap;
        private RectTransform navmapRectTransform = null;
        private Vector3 mouseMapCoords;

        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());
        public MapAtlas atlas;
        public RawImage parcelHighlightImage;
        public TextMeshProUGUI highlightedParcelText;

        public Image playerPositionIcon;
        public HashSet<MinimapMetadata.MinimapSceneInfo> scenesOfInterest = new HashSet<MinimapMetadata.MinimapSceneInfo>();
        public Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject> scenesOfInterestMarkers = new Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject>();

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

        private void Awake()
        {
            i = this;
        }

        void Start()
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += MapRenderer_OnSceneInfoUpdated;
            playerWorldPosition.OnChange += OnCharacterMove;
            playerRotation.OnChange += OnCharacterRotate;

            worldmapOffset = new Vector3(LEFT_BORDER_PARCELS + WORLDMAP_WIDTH_IN_PARCELS / 2, BOTTOM_BORDER_PARCELS + WORLDMAP_WIDTH_IN_PARCELS / 2, 0);
            parcelSizeInMap = (MapUtils.PARCEL_SIZE / 2);

            navmapRectTransform = atlas.chunksParent.transform as RectTransform;
            parcelHighlightImage.rectTransform.localScale = new Vector3(parcelHightlightScale, parcelHightlightScale, 1f);
        }

        void Update()
        {
            if (!parcelHighlightEnabled) return;

            // Get the world-space corners of the map
            navmapRectTransform.GetWorldCorners(mapWorldspaceCorners);

            // Offset world coordinates origin position in map with border-parcels and worldmap amount of parcels (horizontally/vertically) / 2
            // (since the "border-parcels" outside the world are not the same amount on the 4 sides of the worldmap we can't just use the center of the rect)
            worldCoordsOriginInMap = mapWorldspaceCorners[0] + worldmapOffset * parcelSizeInMap;

            UpdateMouseMapCoords();

            UpdateParcelHighlight();
        }

        void UpdateMouseMapCoords()
        {
            mouseMapCoords = Input.mousePosition - worldCoordsOriginInMap;
            mouseMapCoords = mouseMapCoords / parcelSizeInMap;

            mouseMapCoords.x = (int)Mathf.Floor(mouseMapCoords.x);
            mouseMapCoords.y = (int)Mathf.Floor(mouseMapCoords.y);
        }

        void UpdateParcelHighlight()
        {
            if (!CoordinatesAreInsideTheworld((int)mouseMapCoords.x, (int)mouseMapCoords.y))
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

        bool CoordinatesAreInsideTheworld(int xCoord, int yCoord)
        {
            return (Mathf.Abs(xCoord) <= WORLDMAP_WIDTH_IN_PARCELS / 2) && (Mathf.Abs(yCoord) <= WORLDMAP_WIDTH_IN_PARCELS / 2);
        }

        private void MapRenderer_OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            //TODO(Brian): Add markers.
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
    }
}
