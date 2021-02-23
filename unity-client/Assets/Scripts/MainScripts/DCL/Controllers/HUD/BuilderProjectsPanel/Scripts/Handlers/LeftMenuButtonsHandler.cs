using System;

internal class LeftMenuButtonsHandler : IDisposable
{
    private readonly BuilderProjectsPanelView view;
    private readonly SectionsController sectionsController;

    public LeftMenuButtonsHandler(BuilderProjectsPanelView view, SectionsController sectionsController)
    {
        this.view = view;
        this.sectionsController = sectionsController;

        view.OnScenesToggleChanged += OnSceneToggleChanged;
        view.OnInWorldScenesToggleChanged += OnInWorldScenesToggleChanged;
        view.OnProjectsToggleChanged += OnProjectsToggleChanged;
        view.OnLandToggleChanged += OnLandToggleChanged;

        sectionsController.OnRequestOpenSection += OnRequestOpenSection;
    }

    public void Dispose()
    {
        view.OnScenesToggleChanged -= OnSceneToggleChanged;
        view.OnInWorldScenesToggleChanged -= OnInWorldScenesToggleChanged;
        view.OnProjectsToggleChanged -= OnProjectsToggleChanged;
        view.OnLandToggleChanged -= OnLandToggleChanged;

        sectionsController.OnRequestOpenSection -= OnRequestOpenSection;
    }

    void OnSceneToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
    }

    void OnInWorldScenesToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_DEPLOYED);
    }

    void OnProjectsToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_PROJECT);
    }

    void OnLandToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.LAND);
    }

    void OnRequestOpenSection(SectionsController.SectionId id)
    {
        switch (id)
        {
            case SectionsController.SectionId.SCENES_MAIN:
                view.scenesToggle.isOn = true;
                break;
            case SectionsController.SectionId.SCENES_DEPLOYED:
                view.inWorldScenesToggle.isOn = true;
                break;
            case SectionsController.SectionId.SCENES_PROJECT:
                view.projectsToggle.isOn = true;
                break;
            case SectionsController.SectionId.LAND:
                break;
        }
    }
}
