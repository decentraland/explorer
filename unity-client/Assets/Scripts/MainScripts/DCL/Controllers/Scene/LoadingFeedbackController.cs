using DCL;
using DCL.Controllers;
using DCL.Interface;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGLTF;

/// <summary>
/// This class recopiles all the needed information for show the feedback during the world loading
/// </summary>
public class LoadingFeedbackController : MonoBehaviour
{
    private Model model;

    public class Model
    {
        public List<SceneLoadingStatus> loadedScenes;

        public class SceneLoadingStatus
        {
            public int sceneId;
            public int componentsLoading;
        }
    }

    private void Start()
    {
        model = new Model();
        model.loadedScenes = new List<Model.SceneLoadingStatus>();

        SceneController.i.OnNewSceneAdded += SceneController_OnNewSceneAdded;
        GLTFComponent.OnDownloadingCountChange += GLTFComponent_OnDownloadingCountChange;
        AssetPromise_AB.OnConcurrentRequestsChange += AssetPromise_AB_OnConcurrentRequestsChange;
        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
    }

    private void OnDestroy()
    {
        SceneController.i.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
        GLTFComponent.OnDownloadingCountChange -= GLTFComponent_OnDownloadingCountChange;
        AssetPromise_AB.OnConcurrentRequestsChange -= AssetPromise_AB_OnConcurrentRequestsChange;
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    private void SceneController_OnNewSceneAdded(ParcelScene scene)
    {
        scene.OnStateRefreshed += Scene_OnStateRefreshed;
    }

    private void Scene_OnStateRefreshed(ParcelScene scene)
    {
        Model.SceneLoadingStatus refreshedScene = new Model.SceneLoadingStatus
        {
            sceneId = scene.GetInstanceID(),
            componentsLoading = scene.disposableNotReadyCount
        };

        switch (scene.currentState)
        {
            case ParcelScene.State.WAITING_FOR_COMPONENTS:
                AddOrUpdateLoadedScene(refreshedScene);
                RefreshFeedbackMessage();
                break;
            case ParcelScene.State.READY:
                scene.OnStateRefreshed -= Scene_OnStateRefreshed;
                RefreshFeedbackMessage();
                break;
        }
    }

    private void AddOrUpdateLoadedScene(Model.SceneLoadingStatus scene)
    {
        Model.SceneLoadingStatus existingScene = model.loadedScenes.FirstOrDefault(x => x.sceneId == scene.sceneId);
        if (existingScene == null)
            model.loadedScenes.Add(scene);
        else
        {
            existingScene.componentsLoading = scene.componentsLoading;
        }
    }

    private void GLTFComponent_OnDownloadingCountChange(int newDownloadingCount)
    {
        RefreshFeedbackMessage();
    }

    private void AssetPromise_AB_OnConcurrentRequestsChange(int newConcurrentRequests)
    {
        RefreshFeedbackMessage();
    }

    private void RefreshFeedbackMessage()
    {
        if (CommonScriptableObjects.rendererState.Get())
            return;

        string loadingText = string.Empty;
        string secondLoadingText = string.Empty;
        int currentComponentsLoading = model.loadedScenes.Sum(x => x.componentsLoading);
        int totalActiveDownloads = AssetPromiseKeeper_GLTF.i.waitingPromisesCount + AssetPromiseKeeper_AB.i.waitingPromisesCount;

        if (currentComponentsLoading > 0)
        {
            loadingText = string.Format(
                "Loading scenes ({0} component{1} left...)",
                currentComponentsLoading,
                currentComponentsLoading > 1 ? "s" : string.Empty);
        }

        if (totalActiveDownloads > 0)
        {
            secondLoadingText = string.Format(
                "Downloading assets ({0} asset{1} left...)",
                totalActiveDownloads,
                totalActiveDownloads > 1 ? "s" : string.Empty);

            if (!string.IsNullOrEmpty(loadingText))
            {
                loadingText += "\\n";
            }

            loadingText += secondLoadingText;
        }

        if (!string.IsNullOrEmpty(loadingText))
        {
            WebInterface.ScenesLoadingFeedback(loadingText);
        }
    }

    private void RendererState_OnChange(bool current, bool previous)
    {
        if (!current)
            return;

        WebInterface.ScenesLoadingFeedback(string.Empty);
        model.loadedScenes.Clear();
    }
}
