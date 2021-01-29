using System.Collections.Generic;
using UnityEngine;

internal class SectionScenesController : SectionBase, IDeployedSceneListener, IProjectSceneListener
{
    public override SectionsController.SectionId id => SectionsController.SectionId.SCENES_MAIN;

    private SectionScenesView view;

    public SectionScenesController(GameObject viewGO) : base(viewGO)
    {
        view = viewGO.GetComponent<SectionScenesView>();
    }

    public override void Dispose()
    {
        Object.Destroy(viewGO);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        throw new System.NotImplementedException();
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        throw new System.NotImplementedException();
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        throw new System.NotImplementedException();
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        throw new System.NotImplementedException();
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        throw new System.NotImplementedException();
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        throw new System.NotImplementedException();
    }
}
