using DCL.Components;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityGLTF.Loader;

namespace UnityGLTF
{
    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class GLTFComponent : MonoBehaviour, ILoadable
    {
        public static bool VERBOSE = false;

        public static int maxSimultaneousDownloads = 10;
        public static float nearestDistance = float.MaxValue;
        public static GLTFComponent nearestGLTFComponent;

        public static int downloadingCount;
        public static int totalDownloadedCount;
        public static int queueCount;

        public class Settings
        {
            public bool? useVisualFeedback;
            public bool? initialVisibility;
            public Shader shaderOverride;
            public bool addMaterialsToPersistentCaching;
            public WebRequestLoader.WebRequestLoaderEventAction OnWebRequestStartEvent;
        }

        public string GLTFUri = null;
        public bool Multithreaded = false;
        public bool UseStream = false;
        public bool UseVisualFeedback = true;
        private bool addMaterialsToPersistentCaching = true;

        public int MaximumLod = 300;
        public int Timeout = 8;
        public Material LoadingTextureMaterial;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;

        public bool InitialVisibility
        {
            get { return initialVisibility; }
            set
            {
                initialVisibility = value;
                if (sceneImporter != null)
                {
                    sceneImporter.initialVisibility = value;
                }
            }
        }


        public GameObject loadingPlaceholder;
        public System.Action OnFinishedLoadingAsset;
        public System.Action OnFailedLoadingAsset;

        [HideInInspector] public bool alreadyLoadedAsset = false;
        [HideInInspector] public GameObject loadedAssetRootGameObject;

        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool MaterialsOnly = false;
        [SerializeField] private int RetryCount = 10;
        [SerializeField] private float RetryTimeout = 2.0f;
        [SerializeField] public Shader shaderOverride = null;
        private bool initialVisibility = true;

        private enum State
        {
            NONE,
            QUEUED,
            DOWNLOADING,
            COMPLETED,
            FAILED
        }

        private State state = State.NONE;

        private bool alreadyDecrementedRefCount;
        private AsyncCoroutineHelper asyncCoroutineHelper;
        private CoroutineStarter.Coroutine loadingRoutine = null;
        private GLTFSceneImporter sceneImporter;
        private Camera mainCamera;

        ILoader loader = null;
        public WebRequestLoader.WebRequestLoaderEventAction OnWebRequestStartEvent;
        public Action OnSuccess { get { return OnFinishedLoadingAsset; } set { OnFinishedLoadingAsset = value; } }
        public Action OnFail { get { return OnFailedLoadingAsset; } set { OnFailedLoadingAsset = value; } }

        public void LoadAsset(string incomingURI = "", bool loadEvenIfAlreadyLoaded = false, Settings settings = null)
        {
            if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded)
            {
                return;
            }

            if (!string.IsNullOrEmpty(incomingURI))
            {
                GLTFUri = incomingURI;
            }

            if (loadingRoutine != null)
            {
                CoroutineStarter.Stop(loadingRoutine);
            }

            alreadyDecrementedRefCount = false;
            state = State.NONE;
            mainCamera = Camera.main;

            if (settings != null)
            {
                ApplySettings(settings);
            }

            loadingRoutine = DCL.CoroutineHelpers.StartThrowingCoroutine(this, LoadAssetCoroutine(), OnFail_Internal);
        }

        void ApplySettings(Settings settings)
        {
            if (settings.initialVisibility.HasValue)
            {
                this.InitialVisibility = settings.initialVisibility.Value;
            }

            if (settings.useVisualFeedback.HasValue)
            {
                this.UseVisualFeedback = settings.useVisualFeedback.Value;
            }

            if (settings.shaderOverride != null)
            {
                this.shaderOverride = settings.shaderOverride;
            }

            if (settings.OnWebRequestStartEvent != null)
            {
                OnWebRequestStartEvent = settings.OnWebRequestStartEvent;
            }

            this.addMaterialsToPersistentCaching = settings.addMaterialsToPersistentCaching;
        }

        private void OnFail_Internal(Exception obj)
        {
            if (state == State.FAILED)
            {
                return;
            }

            state = State.FAILED;

            DecrementDownloadCount();

            OnFailedLoadingAsset?.Invoke();

            if (obj != null)
            {
                if (obj is IndexOutOfRangeException)
                {
                    Destroy(gameObject);
                }

                Debug.Log("GLTF Failure " + obj.ToString() + " ... url = " + this.GLTFUri);
            }
        }

        private void IncrementDownloadCount()
        {
            downloadingCount++;

            if (VERBOSE)
            {
                Debug.Log($"downloadingCount++ = {downloadingCount}");
            }
        }

        private void DecrementDownloadCount()
        {
            if (!alreadyDecrementedRefCount && state != State.NONE && state != State.QUEUED)
            {
                downloadingCount--;
                alreadyDecrementedRefCount = true;
                if (VERBOSE)
                {
                    Debug.Log($"(ERROR) downloadingCount-- = {downloadingCount}");
                }
            }
        }

        public IEnumerator LoadAssetCoroutine()
        {
            if (!string.IsNullOrEmpty(GLTFUri))
            {
                if (VERBOSE)
                {
                    Debug.Log("LoadAssetCoroutine() GLTFUri ->" + GLTFUri);
                }

                asyncCoroutineHelper = gameObject.GetComponent<AsyncCoroutineHelper>() ?? gameObject.AddComponent<AsyncCoroutineHelper>();

                sceneImporter = null;

                Destroy(loadedAssetRootGameObject);

                try
                {
                    if (UseStream)
                    {
                        // Path.Combine treats paths that start with the separator character
                        // as absolute paths, ignoring the first path passed in. This removes
                        // that character to properly handle a filename written with it.
                        GLTFUri = GLTFUri.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                        string fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri);
                        string directoryPath = URIHelper.GetDirectoryName(fullPath);
                        loader = new FileLoader(directoryPath);
                        sceneImporter = new GLTFSceneImporter(
                            Path.GetFileName(GLTFUri),
                            loader,
                            asyncCoroutineHelper
                        );
                    }
                    else
                    {
                        loader = new WebRequestLoader("");

                        if (OnWebRequestStartEvent != null)
                        {
                            (loader as WebRequestLoader).OnLoadStreamStart += OnWebRequestStartEvent;
                        }

                        sceneImporter = new GLTFSceneImporter(
                            GLTFUri,
                            loader,
                            asyncCoroutineHelper
                        );
                    }

                    if (sceneImporter.CreatedObject != null)
                    {
                        Destroy(sceneImporter.CreatedObject);
                    }

                    sceneImporter.SceneParent = gameObject.transform;
                    sceneImporter.Collider = Collider;
                    sceneImporter.maximumLod = MaximumLod;
                    sceneImporter.Timeout = Timeout;
                    sceneImporter.isMultithreaded = Multithreaded;
                    sceneImporter.useMaterialTransition = UseVisualFeedback;
                    sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;
                    sceneImporter.LoadingTextureMaterial = LoadingTextureMaterial;
                    sceneImporter.initialVisibility = initialVisibility;
                    sceneImporter.addMaterialsToPersistentCaching = addMaterialsToPersistentCaching;

                    float time = Time.realtimeSinceStartup;

                    queueCount++;

                    state = State.QUEUED;

                    Func<bool> funcTestDistance = () => TestDistance();
                    yield return new WaitUntil(funcTestDistance);

                    queueCount--;
                    totalDownloadedCount++;

                    IncrementDownloadCount();

                    state = State.DOWNLOADING;
                    yield return sceneImporter.LoadScene(-1);

                    state = State.COMPLETED;

                    DecrementDownloadCount();

                    // Override the shaders on all materials if a shader is provided
                    if (shaderOverride != null)
                    {
                        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
                            renderer.sharedMaterial.shader = shaderOverride;
                        }
                    }
                }
                finally
                {
                    if (loader != null)
                    {
                        if (sceneImporter == null)
                        {
                            Debug.Log("sceneImporter is null, could be due to an invalid URI.", this);
                        }
                        else
                        {
                            loadedAssetRootGameObject = sceneImporter.CreatedObject;

                            sceneImporter?.Dispose();
                            sceneImporter = null;
                        }

                        if (OnWebRequestStartEvent != null)
                        {
                            (loader as WebRequestLoader).OnLoadStreamStart -= OnWebRequestStartEvent;
                            OnWebRequestStartEvent = null;
                        }

                        loader = null;
                    }

                    alreadyLoadedAsset = true;
                    OnFinishedLoadingAsset?.Invoke();
                }
            }
            else
            {
                Debug.Log("couldn't load GLTF because url is empty");
            }

            CoroutineStarter.Stop(loadingRoutine);
            loadingRoutine = null;
            Destroy(loadingPlaceholder);
            Destroy(this);
        }

        private bool TestDistance()
        {
            if (mainCamera == null)
                return true;

            float dist = Vector3.Distance(mainCamera.transform.position, transform.position);

            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestGLTFComponent = this;
            }

            bool result = nearestGLTFComponent == this && downloadingCount < maxSimultaneousDownloads;

            if (result)
            {
                //NOTE(Brian): Reset values so the other GLTFComponents running this coroutine compete again
                //             for distance.
                nearestGLTFComponent = null;
                nearestDistance = float.MaxValue;
            }

            return result;
        }

        public void Load(string url)
        {
            throw new NotImplementedException();
        }

#if UNITY_EDITOR
        // In production it will always be false
        private bool isQuitting = false;

        // We need to check if application is quitting in editor
        // to prevent the pool from releasing objects that are
        // being destroyed 
        void Awake()
        {
            Application.quitting += OnIsQuitting;
        }

        void OnIsQuitting()
        {
            Application.quitting -= OnIsQuitting;
            isQuitting = true;
        }
#endif
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif
            if (sceneImporter != null)
            {
                sceneImporter.Dispose();
            }

            if (!alreadyLoadedAsset && loadingRoutine != null)
            {
                CoroutineStarter.Stop(loadingRoutine);
                OnFail_Internal(null);
                return;
            }

            DecrementDownloadCount();
        }
    }
}
