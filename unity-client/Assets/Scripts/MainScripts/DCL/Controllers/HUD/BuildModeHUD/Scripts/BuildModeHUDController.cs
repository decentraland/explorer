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

    private BuildModeHUDView view;

    private bool areExtraButtonsVisible = false,
                 isControlsVisible = false, 
                 isEntityListVisible = false, 
                 isSceneLimitInfoVisibile = false,
                 isCatalogOpen = false;

    private TooltipController tooltipController;
    private SceneCatalogController sceneCatalogController;
    private QuickBarController quickBarController;
    private EntityInformationController entityInformationController;
    private FirstPersonModeController firstPersonModeController;
    private ShortcutsController shortcutsController;
    private PublishPopupController publishPopupController;
    private DragAndDropSceneObjectController dragAndDropSceneObjectController;
    private PublishBtnController publishBtnController;
    private InspectorBtnController inspectorBtnController;
    private CatalogBtnController catalogBtnController;
    private InspectorController inspectorController;
    private TopActionsButtonsController topActionsButtonsController;
    private CatalogItemDropController catalogItemDropController;

    public BuildModeHUDController()
    {
        CreateBuildModeControllers();
        CreateParentView();
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

    internal void CreateParentView()
    {
        view = BuildModeHUDView.Create();
        view.gameObject.SetActive(false);
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

    internal void ConfigureSceneCatalogController()
    {
        sceneCatalogController.OnHideCatalogClicked += ChangeVisibilityOfCatalog;
        sceneCatalogController.OnCatalogItemSelected += CatalogItemSelected;
        sceneCatalogController.OnStopInput += () => OnStopInput?.Invoke();
        sceneCatalogController.OnResumeInput += () => OnResumeInput?.Invoke();
    }

    internal void ConfigureEntityInformationController()
    {
        entityInformationController.OnPositionChange += (x) => OnSelectedObjectPositionChange?.Invoke(x);
        entityInformationController.OnRotationChange += (x) => OnSelectedObjectRotationChange?.Invoke(x);
        entityInformationController.OnScaleChange += (x) => OnSelectedObjectScaleChange?.Invoke(x);
        entityInformationController.OnNameChange += (entity, newName) => OnEntityRename?.Invoke(entity, newName);
        entityInformationController.OnSmartItemComponentUpdate += (entity) => OnEntitySmartItemComponentUpdate?.Invoke(entity);
    }

    internal void ConfigureFirstPersonModeController()
    {
        firstPersonModeController.OnClick += () => OnChangeModeAction?.Invoke();
    }

    internal void ConfigureShortcutsController()
    {
        shortcutsController.OnCloseClick += ChangeVisibilityOfControls;
    }

    internal void ConfigureDragAndDropSceneObjectController()
    {
        dragAndDropSceneObjectController.OnDrop += () => SceneObjectDroppedInView();
    }

    internal void ConfigurePublishBtnController()
    {
        publishBtnController.OnClick += () => OnPublishAction?.Invoke();
    }

    internal void ConfigureInspectorBtnController()
    {
        inspectorBtnController.OnClick += () => ChangeVisibilityOfEntityList();
    }

    internal void ConfigureCatalogBtnController()
    {
        catalogBtnController.OnClick += ChangeVisibilityOfCatalog;
    }

    internal void ConfigureInspectorController()
    {
        inspectorController.OnEntityClick += (x) => OnEntityClick(x);
        inspectorController.OnEntityDelete += (x) => OnEntityDelete(x);
        inspectorController.OnEntityLock += (x) => OnEntityLock(x);
        inspectorController.OnEntityChangeVisibility += (x) => OnEntityChangeVisibility(x);
        inspectorController.OnEntityRename += (entity, newName) => OnEntityRename(entity, newName);
        inspectorController.SetCloseButtonsAction(ChangeVisibilityOfEntityList);
    }

    internal void ConfigureTopActionsButtonsController()
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

    internal void ConfigureCatalogItemDropController()
    {
        catalogItemDropController.catalogGroupListView = view.sceneCatalogView.catalogGroupListView;
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

    void CatalogItemSelected(CatalogItem catalogItem)
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
        view.entityInformationView.smartItemListView.SetEntityList(entityList);
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
        if (!view)
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
        if (view)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void ToggleVisibility()
    {
        SetVisibility(!IsVisible());
    }

    public bool IsVisible()
    {
        if (!view)
            return false;

        return view.isShowHideAnimatorVisible;
    }

    public void SceneObjectDroppedInView()
    {
        catalogItemDropController.CatalogitemDropped();
    }
}
