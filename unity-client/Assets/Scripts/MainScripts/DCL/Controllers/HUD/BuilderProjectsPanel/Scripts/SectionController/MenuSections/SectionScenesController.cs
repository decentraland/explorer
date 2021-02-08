using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class SectionScenesController : SectionBase, IDeployedSceneListener, IProjectSceneListener
{
    internal const int MAX_CARDS = 3;
    internal readonly SectionScenesView view;

    private bool hasScenes = false;

    public override ISectionSearchHandler searchHandler => hasScenes ? sceneSearchHandler : null;
    public override SearchBarConfig searchBarConfig => new SearchBarConfig()
    {
        showFilterContributor = false,
        showFilterOperator = false,
        showFilterOwner = false,
        showResultLabel = false
    };

    private readonly SceneSearchHandler sceneSearchHandler = new SceneSearchHandler();

    private Dictionary<string, SceneCardView> deployedViews;
    private Dictionary<string, SceneCardView> projectViews;
    private List<SearchInfoScene> searchList = new List<SearchInfoScene>();

    public SectionScenesController()
    {
        var prefab = Resources.Load<SectionScenesView>("BuilderProjectsPanelMenuSections/SectionScenesView");
        view = Object.Instantiate(prefab);
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
        searchList.Clear();
    }

    private void ViewDirty()
    {
        bool hasDeployedScenes = view.deployedSceneContainer.childCount > 0;
        bool hasProjectScenes = view.projectSceneContainer.childCount > 0;
        hasScenes = hasDeployedScenes || hasProjectScenes;

        view.contentScreen.SetActive(hasScenes);
        view.emptyScreen.SetActive(!hasScenes);
        view.inWorldContainer.SetActive(hasDeployedScenes);
        view.projectsContainer.SetActive(hasProjectScenes);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        deployedViews = scenes;
        searchList.AddRange(scenes.Values.Select(scene => scene.searchInfo));
        sceneSearchHandler.SetSearchableList(searchList);
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        projectViews = scenes;
        searchList.AddRange(scenes.Values.Select(scene => scene.searchInfo));
        sceneSearchHandler.SetSearchableList(searchList);
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        sceneSearchHandler.RemoveItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        sceneSearchHandler.RemoveItem(scene.searchInfo);
    }

    private void OnSearchResult(List<SearchInfoScene> searchInfoScenes)
    {
        if (deployedViews != null)
            SetResult(deployedViews, searchInfoScenes, view.deployedSceneContainer);

        if (projectViews != null)
            SetResult(projectViews, searchInfoScenes, view.projectSceneContainer);

        ViewDirty();
    }

    private void SetResult(Dictionary<string, SceneCardView> scenesViews, List<SearchInfoScene> searchInfoScenes,
        Transform parent)
    {
        int count = 0;

        for (int i = 0; i < searchInfoScenes.Count; i++)
        {
            if (scenesViews.TryGetValue(searchInfoScenes[i].id, out SceneCardView sceneView))
            {
                sceneView.SetParent(parent);
                sceneView.transform.SetSiblingIndex(count);
                sceneView.gameObject.SetActive(false);
                count++;
            }
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(i < count && i < MAX_CARDS);
        }
    }
}
