using DCL.Controllers;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeHUDController : IHUD
{
    public event Action OnChangeModeAction;
    public event Action OnTranslateSelectedAction;
    public event Action OnRotateSelectedAction;
    public event Action OnScaleSelectedAction;
    public event Action OnResetAction;
    public event Action OnDuplicateSelectedAction;
    public event Action OnDeleteSelectedAction;
    public event Action OnEntityListVisible;
    public event Action OnStopInput;
    public event Action OnResumeInput;
    public event Action OnTutorialAction;
    public event Action OnPublishAction;
    public event Action OnLogoutAction;
    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action<DCLBuilderInWorldEntity> OnEntityClick;
    public event Action<DCLBuilderInWorldEntity> OnEntityDelete;
    public event Action<DCLBuilderInWorldEntity> OnEntityLock;
    public event Action<DCLBuilderInWorldEntity> OnEntityChangeVisibility;
    public event Action<DCLBuilderInWorldEntity, string> OnEntityRename;
    public event Action<DCLBuilderInWorldEntity> OnEntitySmartItemComponentUpdate;
    public event Action<Vector3> OnSelectedObjectPositionChange;
    public event Action<Vector3> OnSelectedObjectRotationChange;
    public event Action<Vector3> OnSelectedObjectScaleChange;
    public event Action OnCatalogOpen; // Note(Adrian): This is used right now for tutorial purposes

    internal IBuildModeHUDView view;

    internal bool areExtraButtonsVisible = false,
                  isControlsVisible = false, 
                  isEntityListVisible = false, 
                  isSceneLimitInfoVisibile = false,
                  isCatalogOpen = false;

    internal ITooltipController tooltipController;
    internal ISceneCatalogController sceneCatalogController;
    internal IQuickBarController quickBarController;
    internal IEntityInformationController entityInformationController;
    internal IFirstPersonModeController firstPersonModeController;
    internal IShortcutsController shortcutsController;
    internal IPublishPopupController publishPopupController;
    internal IDragAndDropSceneObjectController dragAndDropSceneObjectController;
    internal IPublishBtnController publishBtnController;
    internal IInspectorBtnController inspectorBtnController;
    internal ICatalogBtnController catalogBtnController;
    internal IInspectorController inspectorController;
    internal ITopActionsButtonsController topActionsButtonsController;
    internal CatalogItemDropController catalogItemDropController;

    public BuildModeHUDController()
    {
        CreateBuildModeControllers();
        CreateMainView(BuildModeHUDView.Create());
        ConfigureSceneCatalogController();
        ConfigureEntityInformationController();
        ConfigureFirstPersonModeController();
        ConfigureShortcutsController();
        ConfigureDragAndDropSceneObjectController();
        ConfigurePublishBtnController();
        ConfigureInspectorBtnController();
        ConfigureCatalogBtnController();
        ConfigureInspectorController();
        ConfigureTopActionsButtonsController();
        ConfigureCatalogItemDropController();
    }

    public BuildModeHUDController(
        IBuildModeHUDView buildModeHUDView,
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
        this.sceneCatalogController = sceneCatalogController;
        this.quickBarController = quickBarController;
        this.entityInformationController = entityInformationController;
        this.firstPersonModeController = firstPersonModeController;
        this.shortcutsController = shortcutsController;
        this.publishPopupController = publishPopupController;
        this.dragAndDropSceneObjectController = dragAndDropSceneObjectController;
        this.publishBtnController = publishBtnController;
        this.inspectorBtnController = inspectorBtnController;
        this.catalogBtnController = catalogBtnController;
        this.inspectorController = inspectorController;
        this.topActionsButtonsController = topActionsButtonsController;
        this.catalogItemDropController = new CatalogItemDropController();

        CreateMainView(buildModeHUDView);
    }

    internal void CreateBuildModeControllers()
    {
        tooltipController = new TooltipController();
        sceneCatalogController = new SceneCatalogController();
        quickBarController = new QuickBarController();
        entityInformationController = new EntityInformationController();
        firstPersonModeController = new FirstPersonModeController();
        shortcutsController = new ShortcutsController();
        publishPopupController = new PublishPopupController();
        dragAndDropSceneObjectController = new DragAndDropSceneObjectController();
        publishBtnController = new PublishBtnController();
        inspectorBtnController = new InspectorBtnController();
        catalogBtnController = new CatalogBtnController();
        inspectorController = new InspectorController();
        topActionsButtonsController = new TopActionsButtonsController();
        catalogItemDropController = new CatalogItemDropController();
    }

    internal void CreateMainView(IBuildModeHUDView newView)
    {
        view = newView;
        
        if (view.viewGO != null)
            view.viewGO.SetActive(false);

        view.Initialize(
            tooltipController,
            sceneCatalogController,
            quickBarController,
            entityInformationController,
            firstPersonModeController,
            shortcutsController,
            publishPopupController,
            dragAndDropSceneObjectController,
            publishBtnController,
            inspectorBtnController,
            catalogBtnController,
            inspectorController,
            topActionsButtonsController);
    }

    private void ConfigureSceneCatalogController()
    {
        sceneCatalogController.OnHideCatalogClicked += ChangeVisibilityOfCatalog;
        sceneCatalogController.OnCatalogItemSelected += CatalogItemSelected;
        sceneCatalogController.OnStopInput += () => OnStopInput?.Invoke();
        sceneCatalogController.OnResumeInput += () => OnResumeInput?.Invoke();
    }

    private void ConfigureEntityInformationController()
    {
        entityInformationController.OnPositionChange += (x) => OnSelectedObjectPositionChange?.Invoke(x);
        entityInformationController.OnRotationChange += (x) => OnSelectedObjectRotationChange?.Invoke(x);
        entityInformationController.OnScaleChange += (x) => OnSelectedObjectScaleChange?.Invoke(x);
        entityInformationController.OnNameChange += (entity, newName) => OnEntityRename?.Invoke(entity, newName);
        entityInformationController.OnSmartItemComponentUpdate += (entity) => OnEntitySmartItemComponentUpdate?.Invoke(entity);
    }

    private void ConfigureFirstPersonModeController()
    {
        firstPersonModeController.OnClick += () => OnChangeModeAction?.Invoke();
    }

    private void ConfigureShortcutsController()
    {
        shortcutsController.OnCloseClick += ChangeVisibilityOfControls;
    }

    private void ConfigureDragAndDropSceneObjectController()
    {
        dragAndDropSceneObjectController.OnDrop += () => SceneObjectDroppedInView();
    }

    private void ConfigurePublishBtnController()
    {
        publishBtnController.OnClick += () => OnPublishAction?.Invoke();
    }

    private void ConfigureInspectorBtnController()
    {
        inspectorBtnController.OnClick += () => ChangeVisibilityOfEntityList();
    }

    private void ConfigureCatalogBtnController()
    {
        catalogBtnController.OnClick += ChangeVisibilityOfCatalog;
    }

    private void ConfigureInspectorController()
    {
        inspectorController.OnEntityClick += (x) => OnEntityClick(x);
        inspectorController.OnEntityDelete += (x) => OnEntityDelete(x);
        inspectorController.OnEntityLock += (x) => OnEntityLock(x);
        inspectorController.OnEntityChangeVisibility += (x) => OnEntityChangeVisibility(x);
        inspectorController.OnEntityRename += (entity, newName) => OnEntityRename(entity, newName);
        inspectorController.SetCloseButtonsAction(ChangeVisibilityOfEntityList);
    }

    private void ConfigureTopActionsButtonsController()
    {
        topActionsButtonsController.OnChangeModeClick += () => OnChangeModeAction?.Invoke();
        topActionsButtonsController.OnExtraClick += ChangeVisibilityOfExtraBtns;
        topActionsButtonsController.OnTranslateClick += () => OnTranslateSelectedAction?.Invoke();
        topActionsButtonsController.OnRotateClick += () => OnRotateSelectedAction?.Invoke();
        topActionsButtonsController.OnScaleClick += () => OnScaleSelectedAction?.Invoke();
        topActionsButtonsController.OnResetClick += () => OnResetAction?.Invoke();
        topActionsButtonsController.OnDuplicateClick += () => OnDuplicateSelectedAction?.Invoke();
        topActionsButtonsController.OnDeleteClick += () => OnDeleteSelectedAction?.Invoke();
        topActionsButtonsController.OnLogOutClick += () => OnLogoutAction?.Invoke();
        topActionsButtonsController.extraActionsController.OnControlsClick += ChangeVisibilityOfControls;
        topActionsButtonsController.extraActionsController.OnHideUIClick += ChangeVisibilityOfUI;
        topActionsButtonsController.extraActionsController.OnTutorialClick += () => OnTutorialAction?.Invoke();
    }

    private void ConfigureCatalogItemDropController()
    {
        catalogItemDropController.catalogGroupListView = view.sceneCatalog.catalogGroupListView;
        catalogItemDropController.OnCatalogItemDropped += CatalogItemSelected;
    }

    public void PublishStart()
    {
        view.PublishStart();
    }

    public void PublishEnd(string message)
    {
        view.PublishEnd(message);
    }

    public void SetParcelScene(ParcelScene parcelScene)
    {
        inspectorController.sceneLimitsController.SetParcelScene(parcelScene);
    }

    public void SetPublishBtnAvailability(bool isAvailable)
    {
        view.SetPublishBtnAvailability(isAvailable);
    }

    #region Catalog

    public void RefreshCatalogAssetPack()
    {
        view.RefreshCatalogAssetPack();
    }

    public void RefreshCatalogContent()
    {
        view.RefreshCatalogContent();
    }

    public void CatalogItemSelected(CatalogItem catalogItem)
    {
        OnCatalogItemSelected?.Invoke(catalogItem);
        SetVisibilityOfCatalog(false);
    }

    public void SetVisibilityOfCatalog(bool isVisible)
    {
        isCatalogOpen = isVisible;
        view.SetVisibilityOfCatalog(isCatalogOpen);

        if (isVisible)
            OnCatalogOpen?.Invoke();
    }

    public void ChangeVisibilityOfCatalog()
    {
        isCatalogOpen = !sceneCatalogController.IsCatalogOpen();
        SetVisibilityOfCatalog(isCatalogOpen);
    }

    #endregion

    #region SceneLimitInfo

    public void ShowSceneLimitsPassed()
    {
        if (!isSceneLimitInfoVisibile)
            ChangeVisibilityOfSceneInfo();
    }

    public void UpdateSceneLimitInfo()
    {
        inspectorController.sceneLimitsController.UpdateInfo();
    }

    public void ChangeVisibilityOfSceneInfo(bool shouldBeVisibile)
    {
        isSceneLimitInfoVisibile = shouldBeVisibile;
        view.SetVisibilityOfSceneInfo(isSceneLimitInfoVisibile);
    }

    public void ChangeVisibilityOfSceneInfo()
    {
        isSceneLimitInfoVisibile = !isSceneLimitInfoVisibile;
        view.SetVisibilityOfSceneInfo(isSceneLimitInfoVisibile);
    }

    #endregion

    public void ActivateFirstPersonModeUI()
    {
        if (view != null)
            view.SetFirstPersonView();
    }

    public void ActivateGodModeUI()
    {
        if(view != null)
            view.SetGodModeView();
    }

    #region EntityInformation

    public void EntityInformationSetEntity(DCLBuilderInWorldEntity entity,ParcelScene scene)
    {
        entityInformationController.SetEntity(entity, scene);
    }

    public void ShowEntityInformation()
    {
        entityInformationController.Enable();
    }

    public void HideEntityInformation()
    {
        entityInformationController.Disable();
    }

    #endregion

    public void SetEntityList(List<DCLBuilderInWorldEntity> entityList)
    {
        inspectorController.SetEntityList(entityList);

        if (view.entityInformation != null)
            view.entityInformation.smartItemListView.SetEntityList(entityList);
    }

    public void ChangeVisibilityOfEntityList()
    {
        isEntityListVisible = !isEntityListVisible;
        if (isEntityListVisible)
        {
            OnEntityListVisible?.Invoke();
            inspectorController.OpenEntityList();
        }
        else
        {
            inspectorController.CloseList();
        }
    }

    public void ClearEntityList()
    {
        inspectorController.ClearList();
    }

    public void ChangeVisibilityOfControls()
    {
        isControlsVisible = !isControlsVisible;
        view.SetVisibilityOfControls(isControlsVisible);
    }

    public void ChangeVisibilityOfUI()
    {
        SetVisibility(!IsVisible());
    }

    public void ChangeVisibilityOfExtraBtns()
    {
        areExtraButtonsVisible = !areExtraButtonsVisible;
        view.SetVisibilityOfExtraBtns(areExtraButtonsVisible);
    }

    public void SetVisibility(bool visible)
    {
        if (view == null)
            return;

        if (IsVisible() && !visible)
        {
            view.AnimatorShow(false);
            AudioScriptableObjects.fadeOut.Play(true);
        }
        else if (!IsVisible() && visible)
        {
            view.SetActive(true);
            view.AnimatorShow(true);
            AudioScriptableObjects.fadeIn.Play(true);
        }
    }

    public void Dispose()
    {
        if (view == null)
            return;
        else if (view.viewGO != null)
            UnityEngine.Object.Destroy(view.viewGO);
    }

    public void ToggleVisibility()
    {
        SetVisibility(!IsVisible());
    }

    public bool IsVisible()
    {
        if (view == null)
            return false;

        return view.isShowHideAnimatorVisible;
    }

    public void SceneObjectDroppedInView()
    {
        catalogItemDropController.CatalogitemDropped();
    }
}
