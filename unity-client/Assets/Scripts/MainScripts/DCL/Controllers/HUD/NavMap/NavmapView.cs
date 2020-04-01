using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using TMPro;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        const int LEFT_BORDER_PARCELS = 25;
        const int BOTTOM_BORDER_PARCELS = 25;
        const int WORLDMAP_WIDTH_IN_PARCELS = 300;

        [SerializeField] InputAction_Trigger toggleNavMapAction;
        [SerializeField] Button closeButton;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Transform scrollRectContentTransform;
        [SerializeField] TextMeshProUGUI currentSceneNameText;
        [SerializeField] TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] RawImage parcelHighlightImage;
        InputAction_Trigger.Triggered toggleNavMapDelegate;

        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;
        MinimapMetadata mapMetadata;
        Vector3 worldmapOffset;
        Vector3[] navmapWorldspaceCorners = new Vector3[4];

        [Header("DEBUG")]
        public Vector2 mouseMapCoords;

        // TODO: Remove this bool once we finish the feature
        bool enabledInProduction = false;

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();

            worldmapOffset = new Vector3(LEFT_BORDER_PARCELS + WORLDMAP_WIDTH_IN_PARCELS / 2, BOTTOM_BORDER_PARCELS + WORLDMAP_WIDTH_IN_PARCELS / 2, 0);

            closeButton.onClick.AddListener(() => { ToggleNavMap(); });
            scrollRect.onValueChanged.AddListener((x) => { if (scrollRect.gameObject.activeSelf) MapRenderer.i.atlas.UpdateCulling(); });

            toggleNavMapDelegate = (x) => { ToggleNavMap(); };
            toggleNavMapAction.OnTriggered += toggleNavMapDelegate;

            MinimapHUDView.OnUpdateData += UpdateCurrentSceneData;
        }

        void Update()
        {
            if (!scrollRect.gameObject.activeSelf) return;

            UpdateMouseMapCoords();

            DrawHoveredScene();
        }

        void UpdateMouseMapCoords()
        {
            RectTransform chunksContainerRectTransform = MapRenderer.i.atlas.chunksParent.transform as RectTransform;
            chunksContainerRectTransform.GetWorldCorners(navmapWorldspaceCorners);

            // Offset world coordinates origin position in map with border-parcels and worldmap amount of parcels (horizontally/vertically) / 2
            // (since the "border-parcels" outside the world are not the same amount on the 4 sides of the worldmap we can't just use the center of the rect)

            Vector3 worldCoordsOriginInMap = navmapWorldspaceCorners[0];
            worldCoordsOriginInMap += worldmapOffset * (MapUtils.PARCEL_SIZE / 2);
            // worldCoordsOriginInMap.x += worldmapOffset.x * MapUtils.PARCEL_SIZE / 2;
            // worldCoordsOriginInMap.y += worldmapOffset.y * MapUtils.PARCEL_SIZE / 2;

            Rect newRect = new Rect(worldCoordsOriginInMap, navmapWorldspaceCorners[2] - worldCoordsOriginInMap);
            mouseMapCoords = Input.mousePosition - worldCoordsOriginInMap;
            mouseMapCoords = mouseMapCoords / 10;
            // mouseMapCoords.x = (int)mouseMapCoords.x;
            // mouseMapCoords.y = (int)mouseMapCoords.y;
        }

        void DrawHoveredScene()
        {


            // Until we get all the scenes info

            // ----------------------------------------------------

            // var sceneInfo = mapMetadata.GetSceneInfo(mouseMapCoords.x, mouseMapCoords.y);

            // // var sceneInfo = mapMetadata.GetSceneInfo(0, 0);
            // // var sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(100, 100);

            // if (sceneInfo == null)
            //     Debug.Log("SCENE INFO IS NULL!!!!!");
            // else
            //     Debug.Log(sceneInfo.name);
        }

        void ToggleNavMap()
        {
            if (MapRenderer.i == null) return;

#if !UNITY_EDITOR
            if(!enabledInProduction) return;
#endif

            scrollRect.StopMovement();
            scrollRect.gameObject.SetActive(!scrollRect.gameObject.activeSelf);

            if (scrollRect.gameObject.activeSelf)
            {
                Utils.UnlockCursor();

                minimapViewport = MapRenderer.i.atlas.viewport;
                mapRendererMinimapParent = MapRenderer.i.transform.parent;
                atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRectContentTransform);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;

                // Reposition de player icon parent to scroll everything together
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(scrollRect.content);

                // Center map
                MapRenderer.i.atlas.CenterToTile(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition));
            }
            else
            {
                Utils.LockCursor();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(MapRenderer.i.atlas.chunksParent.transform.parent);
                (MapRenderer.i.atlas.overlayLayerGameobject.transform as RectTransform).anchoredPosition = Vector2.zero;

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition.Get()));
            }
        }

        void UpdateCurrentSceneData(MinimapHUDModel model)
        {
            currentSceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
            currentSceneCoordsText.text = model.playerPosition;
        }
    }
}