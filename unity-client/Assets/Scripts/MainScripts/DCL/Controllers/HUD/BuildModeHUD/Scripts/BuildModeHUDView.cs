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
    private IInspectorController inspectorController;
    [SerializeField] internal TopActionsButtonsView topActionsButtonsView;
    private ITopActionsButtonsController topActionsButtonsController;

    internal bool isShowHideAnimatorVisible => showHideAnimator.isVisible;

    public void Initialize(
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

        this.sceneCatalogController = sceneCatalogController;
        this.sceneCatalogController.Initialize(sceneCatalogView, quickBarController);

        this.quickBarController = quickBarController;
        this.quickBarController.Initialize(quickBarView, sceneCatalogController);

        this.entityInformationController = entityInformationController;
        this.entityInformationController.Initialize(entityInformationView);

        this.firstPersonModeController = firstPersonModeController;
        this.firstPersonModeController.Initialize(firstPersonModeView, tooltipController);

        this.shortcutsController = shortcutsController;
        this.shortcutsController.Initialize(shortcutsView);

        this.publishPopupController = publishPopupController;
        this.publishPopupController.Initialize(publishPopupView);

        this.dragAndDropSceneObjectController = dragAndDropSceneObjectController;
        this.dragAndDropSceneObjectController.Initialize(dragAndDropSceneObjectView);

        this.publishBtnController = publishBtnController;
        this.publishBtnController.Initialize(publishBtnView, tooltipController);

        this.inspectorBtnController = inspectorBtnController;
        this.inspectorBtnController.Initialize(inspectorBtnView, tooltipController);

        this.catalogBtnController = catalogBtnController;
        this.catalogBtnController.Initialize(catalogBtnView, tooltipController);

        this.inspectorController = inspectorController;
        this.inspectorController.Initialize(inspectorView);

        this.topActionsButtonsController = topActionsButtonsController;
        this.topActionsButtonsController.Initialize(topActionsButtonsView, tooltipController);
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

    public void SetVisibilityOfCatalog(bool isVisible)
    {
        if (isVisible)
            sceneCatalogController.OpenCatalog();
        else
            sceneCatalogController.CloseCatalog();
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

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void AnimatorShow(bool isVisible)
    {
        if (isVisible)
            showHideAnimator.Show();
        else
            showHideAnimator.Hide();
    }
}
