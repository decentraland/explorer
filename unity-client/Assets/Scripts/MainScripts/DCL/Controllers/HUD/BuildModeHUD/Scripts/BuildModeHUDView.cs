using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeHUDView : MonoBehaviour
{
    public SceneLimitInfoController sceneLimitInfoController;
    public BuilderInWorldEntityListController entityListController;

    public GameObject firstPersonCanvasGO, godModeCanvasGO, extraBtnsGO;
    public Button changeModeBtn,extraBtn,controlsBtn,closeControlsBtn,hideUIBtn,entityListBtn,catalogBtn;
    public Button translateBtn, rotateBtn, scaleBtn, resetBtn, duplicateBtn, deleteBtn,publishBtn;
    public Button[] closeEntityListBtns;
    public Button tutorialBtn;
    public Button logOutBtn;
    public TextMeshProUGUI publishStatusTxt;

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
        IDragAndDropSceneObjectController dragAndDropSceneObjectController)
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

        this.publishPopupController = publishPopupController;
        this.publishPopupController.Initialize(publishPopupView);

        this.dragAndDropSceneObjectController = dragAndDropSceneObjectController;
        this.dragAndDropSceneObjectController.Initialize(dragAndDropSceneObjectView, buildModeHUDController);
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


        entityListBtn.onClick.AddListener(() => OnEntityListChangeVisibilityAction?.Invoke());

        foreach (Button closeEntityListBtn in closeEntityListBtns)
        {
            closeEntityListBtn.onClick.AddListener(() => OnEntityListChangeVisibilityAction?.Invoke());
        }

        catalogBtn.onClick.AddListener(() => OnSceneCatalogControllerChangeVisibilityAction?.Invoke());
        sceneCatalogView.hideCatalogBtn.onClick.AddListener(() => OnSceneCatalogControllerChangeVisibilityAction?.Invoke());

        changeModeBtn.onClick.AddListener(() => OnChangeModeAction?.Invoke());
        extraBtn.onClick.AddListener(() => OnExtraBtnsClick?.Invoke());
        controlsBtn.onClick.AddListener(() => OnControlsVisibilityAction?.Invoke());
        closeControlsBtn.onClick.AddListener(() => OnControlsVisibilityAction?.Invoke());
        hideUIBtn.onClick.AddListener(() => OnChangeUIVisbilityAction?.Invoke());

        translateBtn.onClick.AddListener(() => OnTranslateSelectionAction?.Invoke());
        rotateBtn.onClick.AddListener(() => OnRotateSelectionAction?.Invoke());
        scaleBtn.onClick.AddListener(() => OnScaleSelectionAction?.Invoke());
        resetBtn.onClick.AddListener(() => OnResetSelectedAction?.Invoke());
        duplicateBtn.onClick.AddListener(() => OnDuplicateSelectionAction?.Invoke());
        deleteBtn.onClick.AddListener(() => OnDeleteSelectionAction?.Invoke());

        sceneCatalogView.catalogGroupListView.OnResumeInput += () => OnResumeInput?.Invoke();
        sceneCatalogView.catalogGroupListView.OnStopInput += () => OnStopInput?.Invoke();

        tutorialBtn.onClick.AddListener(() => OnTutorialAction?.Invoke());
        publishBtn.onClick.AddListener(() => OnPublishAction?.Invoke());
        logOutBtn.onClick.AddListener(() => OnLogoutAction?.Invoke());
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
        publishBtn.interactable = isAvailable;
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
            sceneLimitInfoController.Disable();
        }
        else
        {
            sceneLimitInfoController.Enable();
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
