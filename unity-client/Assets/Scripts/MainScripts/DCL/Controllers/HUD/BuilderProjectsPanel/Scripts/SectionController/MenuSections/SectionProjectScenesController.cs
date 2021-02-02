using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

internal class SectionProjectScenesController : SectionBase, IProjectSceneListener
{
    private readonly SectionProjectScenesView view;

    public SectionProjectScenesController()
    {
        var prefab =
            Resources.Load<SectionProjectScenesView>("BuilderProjectsPanelMenuSections/SectionProjectScenesView");
        view = Object.Instantiate(prefab);

        view.scrollRect.onValueChanged.AddListener((value) => RequestHideContextMenu());
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
        using (var iterator = scenes.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                AddScene(iterator.Current.Value);
            }
        }
        view.scrollRect.verticalNormalizedPosition = 1;
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        AddScene(scene);
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        scene.gameObject.SetActive(false);
    }

    private void AddScene(SceneCardView scene)
    {
        scene.SetParent(view.scenesCardContainer);
        scene.gameObject.SetActive(true);
    }

}
