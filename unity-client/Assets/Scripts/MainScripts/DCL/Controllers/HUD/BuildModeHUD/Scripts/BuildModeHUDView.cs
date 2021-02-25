using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeHUDView : MonoBehaviour
{
    public GameObject firstPersonCanvasGO, godModeCanvasGO, extraBtnsGO;

    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal InputAction_Trigger toggleUIVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleControlsVisibilityInputAction;
    [SerializeField] internal InputAction_Trigger toggleTranslateInputAction;
    [SerializeField] internal InputAction_Trigger toggleRotateInputAction;
    [SerializeField] internal InputAction_Trigger toggleScaleInputAction;
    [SerializeField] internal InputAction_Trigger toggleDuplicateInputAction;
    [SerializeField] internal InputAction_Trigger toggleDeleteInputAction;
    [SerializeField] internal InputAction_Trigger toggleChangeCameraInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetInputAction;
    [SerializeField] internal InputAction_Trigger toggleOpenEntityListInputAction;
    [SerializeField] internal InputAction_Trigger toggleSceneInfoInputAction;
    [SerializeField] internal InputAction_Trigger toggleCatalogInputAction;

    [Header("UI Modules")]
    public TooltipView tooltipView;
    private ITooltipController tooltipController;
    public QuickBarView quickBarView;
    private IQuickBarController quickBarController;
    public SceneCatalogView sceneCatalogView;
    private ISceneCatalogController sceneCatalogController;
    public EntityInformationView entityInformationView;
    private IEntityInformationController entityInformationController;
    public FirstPersonModeView firstPersonModeView;
    private IFirstPersonModeController firstPersonModeController;
    public ShortcutsView shortcutsView;
    private IShortcutsController shortcutsController;
    public PublishPopupView publishPopupView;
    private IPublishPopupController publishPopupController;
    public DragAndDropSceneObjectView dragAndDropSceneObjectView;
    private IDragAndDropSceneObjectController dragAndDropSceneObjectController;
    public PublishBtnView publishBtnView;
    private IPublishBtnController publishBtnController;
    public InspectorBtnView inspectorBtnView;
    private IInspectorBtnController inspectorBtnController;
    public CatalogBtnView catalogBtnView;
    private ICatalogBtnController catalogBtnController;
    public InspectorView inspectorView;
    internal IInspectorController inspectorController;
    public TopActionsButtonsView topActionsButtonsView;
    private ITopActionsButtonsController topActionsButtonsController;

    public event Action OnControlsVisibilityAction, OnChangeUIVisbilityAction, OnTranslateSelectionAction, OnRotateSelectionAction, OnScaleSelectionAction, OnResetSelectedAction, OnDuplicateSelectionAction, OnDeleteSelectionAction;
    public event Action OnChangeModeAction,OnExtraBtnsClick,OnEntityListChangeVisibilityAction,OnSceneLimitInfoControllerChangeVisibilityAction, OnSceneCatalogControllerChangeVisibilityAction;
    public event Action<bool> OnSceneLimitInfoChangeVisibility;

    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action OnStopInput, OnResumeInput,OnTutorialAction,OnPublishAction;
    public event Action OnLogoutAction;
    public event Action OnCatalogItemDrop;

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
        this.sceneCatalogController.OnCatalogItemSelected += (x) => OnCatalogItemSelected?.Invoke(x);

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

        topActionsButtonsView.extraActionsView.controlsBtn.onClick.AddListener(() => OnControlsVisibilityAction?.Invoke());
        topActionsButtonsView.extraActionsView.hideUIBtn.onClick.AddListener(() => OnChangeUIVisbilityAction?.Invoke());
        topActionsButtonsView.extraActionsView.tutorialBtn.onClick.AddListener(() => OnTutorialAction?.Invoke());
    }

    private void Awake()
    {
        toggleUIVisibilityInputAction.OnTriggered += OnUIVisiblityToggleActionTriggered;
        toggleControlsVisibilityInputAction.OnTriggered += OnControlsToggleActionTriggered;

        toggleChangeCameraInputAction.OnTriggered += OnChangeModeActionTriggered;
        toggleTranslateInputAction.OnTriggered += OnTranslateActionTriggered;
        toggleRotateInputAction.OnTriggered += OnRotateActionTriggered;
        toggleScaleInputAction.OnTriggered += OnScaleActionTriggered;
        toggleResetInputAction.OnTriggered += OnResetActionTriggered;
        toggleDuplicateInputAction.OnTriggered += OnDuplicateActionTriggered;
        toggleDeleteInputAction.OnTriggered += OnDeleteActionTriggered;
        toggleOpenEntityListInputAction.OnTriggered += OnEntityListActionTriggered;
        toggleSceneInfoInputAction.OnTriggered += OnSceneLimitInfoControllerChangeVisibilityTriggered;
        toggleCatalogInputAction.OnTriggered += OnSceneCatalogControllerChangeVisibilityTriggered;

        sceneCatalogView.hideCatalogBtn.onClick.AddListener(() => OnSceneCatalogControllerChangeVisibilityAction?.Invoke());
        sceneCatalogView.catalogGroupListView.OnResumeInput += () => OnResumeInput?.Invoke();
        sceneCatalogView.catalogGroupListView.OnStopInput += () => OnStopInput?.Invoke();
    }

    private void OnDestroy()
    {
        toggleUIVisibilityInputAction.OnTriggered -= OnUIVisiblityToggleActionTriggered;
        toggleControlsVisibilityInputAction.OnTriggered -= OnControlsToggleActionTriggered;

        toggleChangeCameraInputAction.OnTriggered -= OnChangeModeActionTriggered;
        toggleTranslateInputAction.OnTriggered -= OnTranslateActionTriggered;
        toggleRotateInputAction.OnTriggered -= OnRotateActionTriggered;
        toggleScaleInputAction.OnTriggered -= OnScaleActionTriggered;
        toggleResetInputAction.OnTriggered -= OnResetActionTriggered;
        toggleDuplicateInputAction.OnTriggered -= OnDuplicateActionTriggered;
        toggleDeleteInputAction.OnTriggered -= OnDeleteActionTriggered;

        toggleOpenEntityListInputAction.OnTriggered -= OnEntityListActionTriggered;
        toggleSceneInfoInputAction.OnTriggered -= OnSceneLimitInfoControllerChangeVisibilityTriggered;
        toggleCatalogInputAction.OnTriggered -= OnSceneCatalogControllerChangeVisibilityTriggered;

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
        extraBtnsGO.SetActive(isVisible);
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

    #region Triggers

    private void OnSceneCatalogControllerChangeVisibilityTriggered(DCLAction_Trigger action)
    {
        OnSceneCatalogControllerChangeVisibilityAction?.Invoke();
    }

    private void OnSceneLimitInfoControllerChangeVisibilityTriggered(DCLAction_Trigger action)
    {
        OnSceneLimitInfoControllerChangeVisibilityAction?.Invoke();
    }

    private void OnEntityListActionTriggered(DCLAction_Trigger action)
    {
        OnEntityListChangeVisibilityAction?.Invoke();
    }
    private void OnResetActionTriggered(DCLAction_Trigger action)
    {
        OnResetSelectedAction?.Invoke();
    }

    private void OnChangeModeActionTriggered(DCLAction_Trigger action)
    {
        OnChangeModeAction?.Invoke();
    }

    private void OnDeleteActionTriggered(DCLAction_Trigger action)
    {
        OnDeleteSelectionAction?.Invoke();
    }

    private void OnDuplicateActionTriggered(DCLAction_Trigger action)
    {
        OnDuplicateSelectionAction?.Invoke();
    }

    private void OnScaleActionTriggered(DCLAction_Trigger action)
    {
        OnScaleSelectionAction?.Invoke();
    }

    private void OnRotateActionTriggered(DCLAction_Trigger action)
    {
        OnRotateSelectionAction?.Invoke();
    }

    private void OnTranslateActionTriggered(DCLAction_Trigger action)
    {
        OnTranslateSelectionAction?.Invoke();
    }

    private void OnControlsToggleActionTriggered(DCLAction_Trigger action)
    {
        OnControlsVisibilityAction?.Invoke();
    }

    private void OnUIVisiblityToggleActionTriggered(DCLAction_Trigger action)
    {
        OnChangeUIVisbilityAction?.Invoke();
    }

    #endregion
}
