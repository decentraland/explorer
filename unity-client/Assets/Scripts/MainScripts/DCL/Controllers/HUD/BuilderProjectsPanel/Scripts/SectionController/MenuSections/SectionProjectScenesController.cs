using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

internal class SectionProjectScenesController : SectionBase, IProjectSceneListener
{
    public override ISectionSearchHandler searchHandler => sceneSearchHandler;

    private readonly SectionProjectScenesView view;

    private readonly SceneSearchHandler sceneSearchHandler = new SceneSearchHandler();
    private Dictionary<string, SceneCardView> scenesViews;

    public SectionProjectScenesController()
    {
        var prefab =
            Resources.Load<SectionProjectScenesView>("BuilderProjectsPanelMenuSections/SectionProjectScenesView");
        view = Object.Instantiate(prefab);

        view.scrollRect.onValueChanged.AddListener((value) => RequestHideContextMenu());
        sceneSearchHandler.OnResult += OnSearchResult;
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.transform.SetParent(viewContainer);
        view.transform.ResetLocalTRS();
    }

    public override void Dispose()
    {
        Object.Destroy(view.gameObject);
    }

    protected override void OnShow()
    {
        view.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        view.gameObject.SetActive(false);
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        scenesViews = scenes;
        sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        scene.gameObject.SetActive(false);
    }

    private void OnSearchResult(List<SearchInfoScene> searchInfoScenes)
    {
        using (var iterator = scenesViews.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetParent(view.scenesCardContainer);

                int index = searchInfoScenes.FindIndex(info => info.id == iterator.Current.Key);
                if (index >= 0)
                {
                    iterator.Current.Value.gameObject.SetActive(true);
                    iterator.Current.Value.transform.SetSiblingIndex(index);
                }
                else
                {
                    iterator.Current.Value.gameObject.SetActive(false);
                }
            }
        }
        view.scrollRect.verticalNormalizedPosition = 1;
    }
}
