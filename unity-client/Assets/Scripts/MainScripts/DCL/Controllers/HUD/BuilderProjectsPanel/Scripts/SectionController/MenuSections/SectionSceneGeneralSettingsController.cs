using UnityEngine;

internal class SectionSceneGeneralSettingsController : SectionBase, ISelectSceneListener
{
    private readonly SectionSceneGeneralSettingsView view;
    
    public SectionSceneGeneralSettingsController()
    {
        var prefab =
            Resources.Load<SectionSceneGeneralSettingsView>("BuilderProjectsPanelMenuSections/SectionSceneGeneralSettingsView");
        view = Object.Instantiate(prefab);
    }

    public override void Dispose()
    {
        Object.Destroy(view.gameObject);
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }
    void ISelectSceneListener.OnSelectScene(ISceneData sceneData)
    {
        view.SetName(sceneData.name);
        view.SetDescription("");
        view.SetConfigurationActive(sceneData.isDeployed);
        view.SetPermissionsActive(sceneData.isDeployed);
    }
}
