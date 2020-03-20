﻿using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using TMPro;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [SerializeField] InputAction_Trigger toggleNavMapAction;
        [SerializeField] Button closeButton;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Transform scrollRectContentTransform;
        [SerializeField] TextMeshProUGUI currentSceneNameText;
        [SerializeField] TextMeshProUGUI currentSceneCoordsText;
        InputAction_Trigger.Triggered toggleNavMapDelegate;

        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;

        void Start()
        {
            closeButton.onClick.AddListener(() => { ToggleNavMap(); });
            scrollRect.onValueChanged.AddListener((x) => { if (scrollRect.gameObject.activeSelf) MapRenderer.i.atlas.UpdateCulling(); });

            toggleNavMapDelegate = (x) => { ToggleNavMap(); };
            toggleNavMapAction.OnTriggered += toggleNavMapDelegate;

            MinimapHUDView.OnUpdateData += UpdateCurrentSceneData;
        }

        void ToggleNavMap()
        {
            if (MapRenderer.i == null) return;

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

                // Reposition de player icon parent to move everything together
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(scrollRect.content);
            }
            else
            {
                Utils.LockCursor();

                scrollRect.StopMovement();


                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(MapRenderer.i.atlas.chunksParent.transform.parent);
            }
        }

        void UpdateCurrentSceneData(MinimapHUDModel model)
        {
            currentSceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
            currentSceneCoordsText.text = model.playerPosition;
        }
    }
}