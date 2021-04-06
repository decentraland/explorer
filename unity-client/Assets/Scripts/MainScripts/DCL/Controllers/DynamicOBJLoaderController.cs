using DCL;
using UnityEngine;

public class DynamicOBJLoaderController : MonoBehaviour
{
    public bool loadOnStart = true;
    public string OBJUrl = "";
    public GameObject loadingPlaceholder;

    public event System.Action OnFinishedLoadingAsset;

    [HideInInspector] public bool alreadyLoadedAsset = false;
    [HideInInspector] public GameObject loadedOBJGameObject;

    WebRequestAsyncOperation loadingOp = null;

    void Awake()
    {
        if (loadOnStart && !string.IsNullOrEmpty(OBJUrl))
        {
            LoadAsset();
        }
    }

    public void LoadAsset(string url = "", bool loadEvenIfAlreadyLoaded = false)
    {
        if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded)
            return;

        if (!string.IsNullOrEmpty(url))
        {
            OBJUrl = url;
        }

        if (loadingOp != null)
        {
            loadingOp.Dispose();
        }

        LoadAssetCoroutine();
    }

    void LoadAssetCoroutine()
    {
        if (!string.IsNullOrEmpty(OBJUrl))
        {
            Destroy(loadedOBJGameObject);

            loadingOp = Environment.i.platform.webRequest.Get(
                OBJUrl,
                (webRequestResult) =>
                {
                    loadedOBJGameObject = OBJLoader.LoadOBJFile(webRequestResult.downloadHandler.text, true);
                    loadedOBJGameObject.name = "LoadedOBJ";
                    loadedOBJGameObject.transform.SetParent(transform);
                    loadedOBJGameObject.transform.localPosition = Vector3.zero;

                    OnFinishedLoadingAsset?.Invoke();
                    alreadyLoadedAsset = true;
                },
                (errorMsg) =>
                {
                    Debug.Log("Couldn't get OBJ, error: " + errorMsg + " ... " + OBJUrl);
                });

            loadingOp.disposeOnCompleted = true;
        }
        else
        {
            Debug.Log("couldn't load OBJ because url is empty");
        }

        if (loadingPlaceholder != null)
        {
            loadingPlaceholder.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (loadingOp != null)
        {
            loadingOp.Dispose();
        }

        Destroy(loadingPlaceholder);
        Destroy(loadedOBJGameObject);
    }
}