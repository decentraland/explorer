using System;
using UnityEngine;

public class BuildModeHUDView : MonoBehaviour
{
    [Header("Main Containers")]
    [SerializeField] internal GameObject firstPersonCanvasGO;
    [SerializeField] internal GameObject godModeCanvasGO;

    [Header("Animator")]
    [SerializeField] internal ShowHideAnimator showHideAnimator;

    [Header("UI Modules")]
    [SerializeField] internal TooltipView tooltipView;
    private ITooltipController tooltipController;
    [SerializeField] internal QuickBarView quickBarView;
    private IQuickBarController quickBarController;
    [SerializeField] internal SceneCatalogView sceneCatalogView;
    private ISceneCatalogController sceneCatalogController;
    [SerializeField] internal EntityInformationView entityInformationView;
    private IEntityInformationController entityInformationController;
    [SerializeField] internal FirstPersonModeView firstPersonModeView;
    private IFirstPersonModeController firstPersonModeController;
    [SerializeField] internal ShortcutsView shortcutsView;
    private IShortcutsController shortcutsController;
    [SerializeField] internal PublishPopupView publishPopupView;
    private IPublishPopupController publishPopupController;
    [SerializeField] internal DragAndDropSceneObjectView dragAndDropSceneObjectView;
    private IDragAndDropSceneObjectController dragAndDropSceneObjectController;
    [SerializeField] internal PublishBtnView publishBtnView;
    private IPublishBtnController publishBtnController;
    [SerializeField] internal InspectorBtnView inspectorBtnView;
    private IInspectorBtnController inspectorBtnController;
    [SerializeField] internal CatalogBtnView catalogBtnView;
    private ICatalogBtnController catalogBtnController;
    [SerializeField] internal InspectorView inspectorView;
    internal IInspectorController inspectorController;
    [SerializeField] internal TopActionsButtonsView topActionsButtonsView;
    private ITopActionsButtonsController topActionsButtonsController;

    public event Action OnControlsVisibilityAction,
                        OnChangeUIVisbilityAction,
                        OnTranslateSelectionAction,
                        OnRotateSelectionAction,
                        OnScaleSelectionAction,
                        OnResetSelectedAction,
                        OnDuplicateSelectionAction,
                        OnDeleteSelectionAction,
                        OnChangeModeAction,
                        OnExtraBtnsClick,
                        OnEntityListChangeVisibilityAction,
                        OnSceneLimitInfoControllerChangeVisibilityAction,
                        OnSceneCatalogControllerChangeVisibilityAction,
                        OnStopInput,
                        OnResumeInput,
                        OnTutorialAction,
                        OnPublishAction,
                        OnLogoutAction,
                        OnCatalogItemDrop;

    public event Action<bool> OnSceneLimitInfoChangeVisibility;
    public event Action<CatalogItem> OnCatalogItemSelected;

    public void Initialize(
        BuildModeHUDController buildModeHUDController,
        ITooltipController tooltipController,
        ISceneCatalogController sceneCatalogController,
        IQuickBarController quickBarController,
        IEntityInformationController entityInformationController,
        IFirstPersonModeController firstPersonModeController,
        IShortcutsController shortcutsController,
        IPublishPopupController publishPopupController,
        IDragAndDropSceneObjectController dragAndDropSceneObjectController,
        IPublishBtnController publishBtnController,
        IInspectorBtnController inspectorBtnController,
        ICatalogBtnController catalogBtnController,
        IInspectorController inspectorController,
        ITopActionsButtonsController topActionsButtonsController)
    {
        this.tooltipController = tooltipController;
        this.tooltipController.Initialize(tooltipView);

        this.quickBarController = quickBarController;
        this.quickBarController.Initialize(quickBarView, sceneCatalogView.catalogGroupListView);

        this.sceneCatalogController = sceneCatalogController;
        this.sceneCatalogController.Initialize(sceneCatalogView, quickBarController);
        this.sceneCatalogController.OnHideCatalogClicked += () => OnSceneCatalogControllerChangeVisibilityAction?.Invoke();
        this.sceneCatalogController.OnCatalogItemSelected += (x) => OnCatalogItemSelected?.Invoke(x);
        this.sceneCatalogController.OnResumeInput += () => OnResumeInput?.Invoke();
        this.sceneCatalogController.OnStopInput += () => OnStopInput?.Invoke();

        this.entityInformationController = entityInformationController;
        this.entityInformationController.Initialize(entityInformationView);

        this.firstPersonModeController = firstPersonModeController;
        this.firstPersonModeController.Initialize(firstPersonModeView, tooltipController);
        firstPersonModeController.OnClick += () => OnChangeModeAction?.Invoke();

        this.shortcutsController = shortcutsController;
        this.shortcutsController.Initialize(shortcutsView);
        shortcutsController.OnCloseClick += () => OnControlsVisibilityAction?.Invoke();

        this.publishPopupController = publishPopupController;
        this.publishPopupController.Initialize(publishPopupView);

        this.dragAndDropSceneObjectController = dragAndDropSceneObjectController;
        this.dragAndDropSceneObjectController.Initialize(dragAndDropSceneObjectView, buildModeHUDController);

        this.publishBtnController = publishBtnController;
        this.publishBtnController.Initialize(publishBtnView, tooltipController);
        publishBtnController.OnClick += () => OnPublishAction?.Invoke();

        this.inspectorBtnController = inspectorBtnController;
        this.inspectorBtnController.Initialize(inspectorBtnView, tooltipController);
        inspectorBtnController.OnClick += () => OnEntityListChangeVisibilityAction?.Invoke();

        this.catalogBtnController = catalogBtnController;
        this.catalogBtnController.Initialize(catalogBtnView, tooltipController);
        catalogBtnController.OnClick += () => OnSceneCatalogControllerChangeVisibilityAction?.Invoke();

        this.inspectorController = inspectorController;
        this.inspectorController.Initialize(inspectorView);
        inspectorController.SetCloseButtonsAction(() => OnEntityListChangeVisibilityAction?.Invoke());

        this.topActionsButtonsController = topActionsButtonsController;
        this.topActionsButtonsController.Initialize(topActionsButtonsView);
        topActionsButtonsController.OnChangeModeClick += () => OnChangeModeAction?.Invoke();
        topActionsButtonsController.OnExtraClick += () => OnExtraBtnsClick?.Invoke();
        topActionsButtonsController.OnTranslateClick += () => OnTranslateSelectionAction?.Invoke();
        topActionsButtonsController.OnRotateClick += () => OnRotateSelectionAction?.Invoke();
        topActionsButtonsController.OnScaleClick += () => OnScaleSelectionAction?.Invoke();
        topActionsButtonsController.OnResetClick += () => OnResetSelectedAction?.Invoke();
        topActionsButtonsController.OnDuplicateClick += () => OnDuplicateSelectionAction?.Invoke();
        topActionsButtonsController.OnDeleteClick += () => OnDeleteSelectionAction?.Invoke();
        topActionsButtonsController.OnLogOutClick += () => OnLogoutAction?.Invoke();
        topActionsButtonsController.extraActionsController.OnControlsClick += () => OnControlsVisibilityAction?.Invoke();
        topActionsButtonsController.extraActionsController.OnHideUIClick += () => OnChangeUIVisbilityAction?.Invoke();
        topActionsButtonsController.extraActionsController.OnTutorialClick += () => OnTutorialAction?.Invoke();
    }

    private void OnDestroy()
    {
        tooltipController.Dispose();
        quickBarController.Dispose();
        sceneCatalogController.Dispose();
        entityInformationController.Dispose();
        firstPersonModeController.Dispose();
        shortcutsController.Dispose();
        publishPopupController.Dispose();
        dragAndDropSceneObjectController.Dispose();
        publishBtnController.Dispose();
        inspectorBtnController.Dispose();
        catalogBtnController.Dispose();
        inspectorController.Dispose();
        topActionsButtonsController.Dispose();
    }

    public void PublishStart()
    {
        publishPopupController.PublishStart();
    }

    public void PublishEnd(string message)
    {
        publishPopupController.PublishEnd(message);
    }

    public void SetPublishBtnAvailability(bool isAvailable)
    {
        publishBtnController.SetInteractable(isAvailable);
    }

    public void RefreshCatalogAssetPack()
    {
        sceneCatalogController.RefreshAssetPack();
    }

    public void RefreshCatalogContent()
    {
        sceneCatalogController.RefreshCatalog();
    }

    public void SceneObjectDroppedInView()
    {
        OnCatalogItemDrop?.Invoke();
    }

    public void SetVisibilityOfCatalog(bool isVisible)
    {
        if (isVisible)
            sceneCatalogController.OpenCatalog();
        else
            sceneCatalogController.CloseCatalog();
    }

    public void ChangeVisibilityOfSceneLimit(bool shouldBeVisible)
    {
        OnSceneLimitInfoChangeVisibility?.Invoke(shouldBeVisible);
    }

    public void SetVisibilityOfSceneInfo(bool isVisible)
    {
        if (!isVisible)
        {
            inspectorController.sceneLimitsController.Disable();
        }
        else
        {
            inspectorController.sceneLimitsController.Enable();
        }
    }

    public void SetVisibilityOfControls(bool isVisible)
    {
        shortcutsController.SetActive(isVisible);
    }

    public void SetVisibilityOfExtraBtns(bool isVisible)
    {
        topActionsButtonsController.extraActionsController.SetActive(isVisible);
    }

    public void SetFirstPersonView()
    {
        firstPersonCanvasGO.SetActive(true);
        godModeCanvasGO.SetActive(false);
        HideToolTip();
    }

    public void SetGodModeView()
    {
        firstPersonCanvasGO.SetActive(false);
        godModeCanvasGO.SetActive(true);
        HideToolTip();
    }

    public void HideToolTip()
    {
        tooltipController.HideTooltip();
    }
}
