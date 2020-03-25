﻿using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MapRenderer : MonoBehaviour
    {
        public static MapRenderer i { get; private set; }

        private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;

        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());

        public MapAtlas atlas;
        public Transform overlayContainer;
        public Image playerPositionIcon;
        public HashSet<MinimapMetadata.MinimapSceneInfo> scenesOfInterest = new HashSet<MinimapMetadata.MinimapSceneInfo>();
        public Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject> scenesOfInterestMarkers = new Dictionary<MinimapMetadata.MinimapSceneInfo, GameObject>();

        private void Awake()
        {
            i = this;
        }

        void Start()
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += MapRenderer_OnSceneInfoUpdated;
            playerWorldPosition.OnChange += OnCharacterMove;
            playerRotation.OnChange += OnCharacterRotate;
        }

        private void MapRenderer_OnSceneInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (scenesOfInterest.Contains(sceneInfo))
                return;

            Debug.Log($"sceneInfo name = {sceneInfo.name} ... type = {sceneInfo.type}");

            if (sceneInfo.isPOI || sceneInfo.type == MinimapMetadata.TileType.Plaza)
            {
                scenesOfInterest.Add(sceneInfo);

                GameObject go = new GameObject(sceneInfo.name);
                go.transform.parent = overlayContainer.transform;

                Vector2 centerTile = Vector2.zero;

                foreach (var parcel in sceneInfo.parcels)
                {
                    centerTile += parcel;
                }

                centerTile /= sceneInfo.parcels.Count;

                go.transform.localPosition = MapUtils.GetTileToLocalPosition(centerTile.x, centerTile.y);
                var text = go.AddComponent<Text>();

                text.text = sceneInfo.name;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

                scenesOfInterestMarkers.Add(sceneInfo, go);
            }
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
