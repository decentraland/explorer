using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using System;
using DCL.Controllers;

public class BuildModeHUDController : IHUD
{
    public event Action OnChangeModeAction,
                        OnTranslateSelectedAction,
                        OnRotateSelectedAction,
                        OnScaleSelectedAction,
                        OnResetAction,
                        OnDuplicateSelectedAction,
                        OnDeleteSelectedAction;

    public event Action OnEntityListVisible,
                        OnStopInput,
                        OnResumeInput,
                        OnTutorialAction,
                        OnPublishAction;

    public event Action<SceneObject> OnSceneObjectSelected;

    public event Action<DecentralandEntityToEdit> OnEntityClick,
                                                  OnEntityDelete,
                                                  OnEntityLock,
                                                  OnEntityChangeVisibility;

    //Note(Adrian): This is used right now for tutorial purposes
    public event Action OnCatalogOpen;

    internal BuildModeHUDView view;
    BuildModeEntityListController buildModeEntityListController;
    bool areExtraButtonsVisible = false,isControlsVisible = false, isEntityListVisible = false, isSceneLimitInfoVisibile = false,isCatalogOpen = false;

    public BuildModeHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("BuildModeHUD")).GetComponent<BuildModeHUDView>();
        view.name = "_BuildModeHUD";
        view.gameObject.SetActive(false);

        buildModeEntityListController = view.GetComponentInChildren<BuildModeEntityListController>();

        buildModeEntityListController.OnEntityClick += (x) => OnEntityClick(x);
        buildModeEntityListController.OnEntityDelete += (x) => OnEntityDelete(x);
        buildModeEntityListController.OnEntityLock += (x) => OnEntityLock(x);
        buildModeEntityListController.OnEntityChangeVisibility += (x) => OnEntityChangeVisibility(x);

        buildModeEntityListController.CloseList();

        view.OnChangeModeAction += () => OnChangeModeAction?.Invoke();
        view.OnExtraBtnsClick += ChangeVisibilityOfExtraBtns;
        view.OnControlsVisibilityAction += ChangeVisibilityOfControls;
        view.OnChangeUIVisbilityAction += ChangeVisibilityOfUI;
        view.OnSceneLimitInfoChangeVisibility += ChangeVisibilityOfSceneInfo;
        view.OnSceneLimitInfoControllerChangeVisibilityAction += ChangeVisibilityOfSceneInfo;
        view.OnSceneCatalogControllerChangeVisibilityAction += ChangeVisibilityOfCatalog;


        view.OnTranslateSelectionAction += () => OnTranslateSelectedAction?.Invoke();
        view.OnRotateSelectionAction += () => OnRotateSelectedAction?.Invoke();
        view.OnScaleSelectionAction += () => OnScaleSelectedAction?.Invoke();
        view.OnResetSelectedAction += () => OnResetAction?.Invoke();
        view.OnDuplicateSelectionAction += () => OnDuplicateSelectedAction?.Invoke();
        view.OnDeleteSelectionAction += () => OnDeleteSelectedAction?.Invoke();

        view.OnSceneObjectSelected += SceneObjectSelected;
        view.OnStopInput += () => OnStopInput?.Invoke();
        view.OnResumeInput += () => OnResumeInput?.Invoke();


        view.OnEntityListChangeVisibilityAction += () => ChangeVisibilityOfEntityList();

        view.OnTutorialAction += () => OnTutorialAction?.Invoke();
        view.OnPublishAction += () => OnPublishAction?.Invoke();
    }

    public void SetParcelScene(ParcelScene parcelScene)
    {
        view.sceneLimitInfoController.SetParcelScene(parcelScene);
    }

    #region Catalog


    void SceneObjectSelected(SceneObject sceneObject)
    {
        OnSceneObjectSelected?.Invoke(sceneObject);
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
        isCatalogOpen = !view.sceneObjectCatalogController.IsCatalogOpen();
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
        view.sceneLimitInfoController.UpdateInfo();
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
        view.SetFirstPersonView();
    }

    public void ActivateGodModeUI()
    {
        view.SetGodModeView();
    }

    public void SetEntityList(List<DecentralandEntityToEdit> entityList)
    {
        buildModeEntityListController.SetEntityList(entityList);
    }

    public void ChangeVisibilityOfEntityList()
    {
        isEntityListVisible = !isEntityListVisible;
        if (isEntityListVisible)
        {
            OnEntityListVisible?.Invoke();
            buildModeEntityListController.OpenEntityList();
        }
        else
        {
            buildModeEntityListController.CloseList();
        }
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

            view.showHideAnimator.Hide();

            AudioScriptableObjects.fadeOut.Play(true);
        }
        else if (!IsVisible() && visible)
        {
            view.gameObject.SetActive(true);
            view.showHideAnimator.Show();
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

        return view.showHideAnimator.isVisible;
    }
}
