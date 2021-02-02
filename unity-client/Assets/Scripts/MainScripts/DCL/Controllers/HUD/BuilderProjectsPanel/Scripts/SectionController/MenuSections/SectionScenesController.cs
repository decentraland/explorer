using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

internal class SectionScenesController : SectionBase, IDeployedSceneListener, IProjectSceneListener
{
    internal const int MAX_CARDS = 3;
    internal readonly SectionScenesView view;

    private int deployedActiveCards = 0;
    private int projectActiveCards = 0;

    public SectionScenesController()
    {
        var prefab = Resources.Load<SectionScenesView>("BuilderProjectsPanelMenuSections/SectionScenesView");
        view = Object.Instantiate(prefab);
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
        deployedActiveCards = 0;
        projectActiveCards = 0;
    }

    private void ViewDirty()
    {
        bool hasDeployedScenes = view.deployedSceneContainer.childCount > 0;
        bool hasProjectScenes = view.projectSceneContainer.childCount > 0;

        view.contentScreen.SetActive(hasDeployedScenes || hasProjectScenes);
        view.emptyScreen.SetActive(!hasDeployedScenes && !hasProjectScenes);
        view.inWorldContainer.SetActive(hasDeployedScenes);
        view.projectsContainer.SetActive(hasProjectScenes);
    }

    private void SetScenes(Dictionary<string, SceneCardView> scenes, Transform container)
    {
        using (var iterator = scenes.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                AddScene(iterator.Current.Value, container, dirty: false);
            }
        }

        ViewDirty();
    }

    private void AddScene(SceneCardView sceneCardView, Transform container, bool dirty = true)
    {
        ref int visibleCardsCount = ref projectActiveCards;
        if (container == view.deployedSceneContainer)
        {
            visibleCardsCount = ref deployedActiveCards;
        }

        bool setVisible = visibleCardsCount < MAX_CARDS;
        sceneCardView.SetParent(container);
        sceneCardView.gameObject.SetActive(setVisible);

        if (setVisible)
        {
            visibleCardsCount++;
        }

        if (dirty)
        {
            ViewDirty();
        }
    }

    private void RemoveScene(SceneCardView sceneCardView, Transform container)
    {
        bool wasVisible = sceneCardView.gameObject.activeSelf;
        sceneCardView.SetParent(null);

        if (wasVisible)
        {
            if (container == view.deployedSceneContainer)
            {
                deployedActiveCards = ActivateNewCardInContainer(container, deployedActiveCards);
            }
            else if (container == view.projectSceneContainer)
            {
                projectActiveCards = ActivateNewCardInContainer(container, projectActiveCards);
            }
        }

        ViewDirty();
    }

    private int ActivateNewCardInContainer(Transform container, int activeCards)
    {
        if (container.transform.childCount < 3)
            return activeCards -1;

        GameObject cardGO;
        for (int i = 0; i < container.childCount; i++)
        {
            cardGO = container.GetChild(i).gameObject;
            if (cardGO.activeSelf)
                continue;

            cardGO.SetActive(true);
            return activeCards;
        }

        return activeCards -1;
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        SetScenes(scenes, view.deployedSceneContainer);
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        SetScenes(scenes, view.projectSceneContainer);
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        AddScene(scene, view.deployedSceneContainer);
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        AddScene(scene, view.projectSceneContainer);
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        RemoveScene(scene, view.deployedSceneContainer);
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        RemoveScene(scene, view.projectSceneContainer);
    }
}
