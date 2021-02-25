using System;

internal class LeftMenuHandler : IDisposable
{
    private readonly BuilderProjectsPanelView view;
    private readonly SectionsController sectionsController;

    private bool isMainPanel = false;
    private SectionsController.SectionId lastMainSectionId;

    public LeftMenuHandler(BuilderProjectsPanelView view, SectionsController sectionsController)
    {
        this.view = view;
        this.sectionsController = sectionsController;

        for (int i = 0; i < view.sectionToggles.Length; i++)
        {
            view.sectionToggles[i].Setup();
        }

        view.backToMainPanelButton.onClick.AddListener(OnSceneSettingsBackPressed);

        sectionsController.OnOpenSectionId += OnOpenSectionId;
        LeftMenuButtonToggleView.OnToggleOn += OnToggleOn;
    }

    public void Dispose()
    {
        view.backToMainPanelButton.onClick.RemoveListener(OnSceneSettingsBackPressed);

        sectionsController.OnOpenSectionId -= OnOpenSectionId;
        LeftMenuButtonToggleView.OnToggleOn -= OnToggleOn;
    }

    public void SetToPreviousMainPanel()
    {
        if (isMainPanel)
            return;

        sectionsController.OpenSection(lastMainSectionId);
    }

    void OnToggleOn(LeftMenuButtonToggleView toggle)
    {
        sectionsController.OpenSection(toggle.openSection);
    }

    void OnOpenSectionId(SectionsController.SectionId sectionId)
    {
        for (int i = 0; i < view.sectionToggles.Length; i++)
        {
            view.sectionToggles[i].SetIsOnWithoutNotify(sectionId == view.sectionToggles[i].openSection);
        }

        bool isMainPanelSection = sectionId == SectionsController.SectionId.SCENES_MAIN ||
                                  sectionId == SectionsController.SectionId.SCENES_DEPLOYED ||
                                  sectionId == SectionsController.SectionId.SCENES_PROJECT ||
                                  sectionId == SectionsController.SectionId.LAND;

        if (isMainPanelSection)
        {
            lastMainSectionId = sectionId;
            SetMainLeftPanel();
        }
        else
        {
            SetSettingsLeftPanel();
        }
    }

    void OnSceneSettingsBackPressed()
    {
        SetToPreviousMainPanel();
    }

    void SetMainLeftPanel()
    {
        if (isMainPanel)
            return;

        isMainPanel = true;
        view.leftPanelMain.SetActive(true);
        view.leftPanelProjectSettings.SetActive(false);
    }

    void SetSettingsLeftPanel()
    {
        if (!isMainPanel)
            return;

        isMainPanel = false;
        view.leftPanelMain.SetActive(false);
        view.leftPanelProjectSettings.SetActive(true);
    }
}
