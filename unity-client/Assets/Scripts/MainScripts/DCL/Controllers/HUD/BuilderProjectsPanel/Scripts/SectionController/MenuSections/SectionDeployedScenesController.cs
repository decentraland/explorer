using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

internal class SectionDeployedScenesController : SectionBase, IDeployedSceneListener
{
    private readonly SectionDeployedScenesView view;

    public SectionDeployedScenesController()
    {
        var prefab =
            Resources.Load<SectionDeployedScenesView>("BuilderProjectsPanelMenuSections/SectionDeployedScenesView");
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

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
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

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        AddScene(scene);
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        scene.gameObject.SetActive(false);
    }

    private void AddScene(SceneCardView scene)
    {
        scene.SetParent(view.scenesCardContainer);
        scene.gameObject.SetActive(true);
    }

}
