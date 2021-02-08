using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class SectionDeployedScenesController : SectionBase, IDeployedSceneListener
{
    public override ISectionSearchHandler searchHandler => sceneSearchHandler;

    private readonly SectionDeployedScenesView view;

    private readonly SceneSearchHandler sceneSearchHandler = new SceneSearchHandler();
    private Dictionary<string, SceneCardView> scenesViews;

    public SectionDeployedScenesController()
    {
        var prefab =
            Resources.Load<SectionDeployedScenesView>("BuilderProjectsPanelMenuSections/SectionDeployedScenesView");
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

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        scenesViews = scenes;
        sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
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
