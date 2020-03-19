using UnityEngine;
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
        [SerializeField] TextMeshProUGUI currentSceneNameText;
        [SerializeField] TextMeshProUGUI currentSceneCoordsText;
        InputAction_Trigger.Triggered toggleNavMapDelegate;

        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;

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

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRect.content);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;
            }
            else
            {
                Utils.LockCursor();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.UpdateCulling();
            }
        }

        void UpdateCurrentSceneData(MinimapHUDModel model)
        {
            currentSceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
            currentSceneCoordsText.text = model.playerPosition;
        }
    }
}