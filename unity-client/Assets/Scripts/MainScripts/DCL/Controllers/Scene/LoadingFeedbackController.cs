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
        public int gltfActiveDownloads;
        public int assetBundlesActiveDownloads;
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
    }

    private void OnDestroy()
    {
        SceneController.i.OnNewSceneAdded -= SceneController_OnNewSceneAdded;
        GLTFComponent.OnDownloadingCountChange -= GLTFComponent_OnDownloadingCountChange;
        AssetPromise_AB.OnConcurrentRequestsChange -= AssetPromise_AB_OnConcurrentRequestsChange;
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
            case ParcelScene.State.NOT_READY:
            case ParcelScene.State.WAITING_FOR_INIT_MESSAGES:
            case ParcelScene.State.WAITING_FOR_COMPONENTS:
                AddOrUpdateLoadedScene(refreshedScene);
                break;
            case ParcelScene.State.READY:
                scene.OnStateRefreshed -= Scene_OnStateRefreshed;
                RemoveLoadedScene(refreshedScene.sceneId);
                break;
        }

        RefreshFeedbackMessage();
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

    private void RemoveLoadedScene(int id)
    {
        model.loadedScenes.RemoveAll(x => x.sceneId == id);
    }

    private void GLTFComponent_OnDownloadingCountChange(int newDownloadingCount)
    {
        model.gltfActiveDownloads = newDownloadingCount;
        RefreshFeedbackMessage();
    }

    private void AssetPromise_AB_OnConcurrentRequestsChange(int newConcurrentRequests)
    {
        model.assetBundlesActiveDownloads = newConcurrentRequests;
        RefreshFeedbackMessage();
    }

    private void RefreshFeedbackMessage()
    {
        if (CommonScriptableObjects.rendererState.Get())
            return;

        string loadingText = "Loading scenes";

        int currentComponentsLoading = model.loadedScenes.Sum(x => x.componentsLoading);
        int totalActiveDownloads = model.gltfActiveDownloads + model.assetBundlesActiveDownloads;

        if (currentComponentsLoading > 0)
        {
            loadingText = string.Format(
                "Loading scenes ({0} component{1} left...)",
                currentComponentsLoading,
                currentComponentsLoading > 1 ? "s" : string.Empty);
        }
        else if (totalActiveDownloads > 0)
        {
            loadingText = string.Format(
                "Downloading assets ({0} asset{1} left...)",
                totalActiveDownloads,
                totalActiveDownloads > 1 ? "s" : string.Empty);
        }

        WebInterface.ScenesLoadingFeedback(loadingText);
    }
}
