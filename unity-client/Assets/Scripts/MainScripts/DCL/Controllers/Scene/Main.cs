using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    public class Main : MonoBehaviour
    {
        public const string EMPTY_GO_POOL_NAME = "Empty";

        public static bool VERBOSE = false;
        public static Main i { get; private set; }

        private EntryPoint_World worldEntryPoint;

        [FormerlySerializedAs("factoryManifest")]
        public DCLComponentFactory componentFactory;

        private SceneController sceneController;
        [System.NonSerialized] public bool prewarmEntitiesPool = true;

        public bool sceneSortDirty = false;
        private bool positionDirty = true;
        private int lastSortFrame = 0;

        public event System.Action OnSortScenes;
        private Vector2Int currentGridSceneCoordinate = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);
        private Vector2Int sortAuxiliaryVector = new Vector2Int(EnvironmentSettings.MORDOR_SCALAR, EnvironmentSettings.MORDOR_SCALAR);


        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

#if !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);
            Debug.unityLogger.logEnabled = false;
#endif

            sceneController.InitializeSceneBoundariesChecker(Environment.i.debugConfig.isDebugMode);

            RenderProfileManifest.i.Initialize();
            Environment.i.Initialize();

            Environment.i.parcelScenesCleaner.Start();

            DCLCharacterController.OnCharacterMoved += SetPositionDirty;

            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneIdChange;

            //TODO(Brian): Move those suscriptions elsewhere.
            PoolManager.i.OnGet -= Environment.i.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet += Environment.i.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.cullingController.objectsTracker.MarkDirty;
            PoolManager.i.OnGet += Environment.i.cullingController.objectsTracker.MarkDirty;

#if !UNITY_EDITOR
            worldEntryPoint = new EntryPoint_World(this); // independent subsystem => put at entrypoint but not at environment
#endif

            // TODO(Brian): This should be fixed when we do the proper initialization layer
            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                Environment.i.cullingController.Start();
            }

            // We trigger the Decentraland logic once SceneController has been instanced and is ready to act.
            WebInterface.StartDecentraland();
        }

        void Start()
        {
            if (prewarmEntitiesPool)
            {
                EnsureEntityPool();
            }

            sceneController.PrewarmPools();
            componentFactory.PrewarmPools();
        }

        void OnDestroy()
        {
            PoolManager.i.OnGet -= Environment.i.physicsSyncController.MarkDirty;
            PoolManager.i.OnGet -= Environment.i.cullingController.objectsTracker.MarkDirty;
            DCLCharacterController.OnCharacterMoved -= SetPositionDirty;
            Environment.i.parcelScenesCleaner.Stop();
            Environment.i.cullingController.Stop();
        }


        private void Update()
        {
            InputController_Legacy.i.Update();

            Environment.i.pointerEventsController.Update();

            if (lastSortFrame != Time.frameCount && sceneSortDirty)
            {
                lastSortFrame = Time.frameCount;
                sceneSortDirty = false;
                SortScenesByDistance();
            }

            Environment.i.performanceMetricsController?.Update();
        }

        private void LateUpdate()
        {
            Environment.i.physicsSyncController.Sync();
        }

        public void EnsureEntityPool() // TODO: Move to PoolManagerFactory
        {
            if (PoolManager.i.ContainsPool(EMPTY_GO_POOL_NAME))
                return;

            GameObject go = new GameObject();
            Pool pool = PoolManager.i.AddPool(EMPTY_GO_POOL_NAME, go, maxPrewarmCount: 2000, isPersistent: true);

            if (prewarmEntitiesPool)
                pool.ForcePrewarm();
        }

        private void SetPositionDirty(DCLCharacterPosition character)
        {
            var currentX = (int) System.Math.Floor(character.worldPosition.x / ParcelSettings.PARCEL_SIZE);
            var currentY = (int) System.Math.Floor(character.worldPosition.z / ParcelSettings.PARCEL_SIZE);

            positionDirty = currentX != currentGridSceneCoordinate.x || currentY != currentGridSceneCoordinate.y;

            if (positionDirty)
            {
                sceneSortDirty = true;
                currentGridSceneCoordinate.x = currentX;
                currentGridSceneCoordinate.y = currentY;

                // Since the first position for the character is not sent from Kernel until just-before calling
                // the rendering activation from Kernel, we need to sort the scenes to get the current scene id
                // to lock the rendering accordingly...
                if (!CommonScriptableObjects.rendererState.Get())
                {
                    SortScenesByDistance();
                }
            }
        }

        private void SortScenesByDistance()
        {
            if (DCLCharacterController.i == null) return;

            WorldState worldState = Environment.i.worldState;

            worldState.currentSceneId = null;
            worldState.scenesSortedByDistance.Sort(SortScenesByDistanceMethod);

            using (var iterator = Environment.i.worldState.scenesSortedByDistance.GetEnumerator())
            {
                ParcelScene scene;
                bool characterIsInsideScene;

                while (iterator.MoveNext())
                {
                    scene = iterator.Current;

                    if (scene == null) continue;

                    characterIsInsideScene = scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition);

                    if (scene.sceneData.id != worldState.globalSceneId && characterIsInsideScene)
                    {
                        worldState.currentSceneId = scene.sceneData.id;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(worldState.currentSceneId))
            {
                // When we don't know the current scene yet, we must lock the rendering from enabling until it is set
                CommonScriptableObjects.rendererState.AddLock(this);
            }
            else
            {
                // 1. Set current scene id
                CommonScriptableObjects.sceneID.Set(worldState.currentSceneId);

                // 2. Attempt to remove SceneController's lock on rendering
                CommonScriptableObjects.rendererState.RemoveLock(this);
            }

            OnSortScenes?.Invoke();
        }

        private int SortScenesByDistanceMethod(ParcelScene sceneA, ParcelScene sceneB)
        {
            sortAuxiliaryVector = sceneA.sceneData.basePosition - currentGridSceneCoordinate;
            int dist1 = sortAuxiliaryVector.sqrMagnitude;

            sortAuxiliaryVector = sceneB.sceneData.basePosition - currentGridSceneCoordinate;
            int dist2 = sortAuxiliaryVector.sqrMagnitude;

            return dist1 - dist2;
        }

        private void OnCurrentSceneIdChange(string newSceneId, string prevSceneId)
        {
            if (Environment.i.worldState.TryGetScene(newSceneId, out ParcelScene newCurrentScene) && !newCurrentScene.isReady)
            {
                CommonScriptableObjects.rendererState.AddLock(newCurrentScene);

                newCurrentScene.OnSceneReady += (readyScene) => { CommonScriptableObjects.rendererState.RemoveLock(readyScene); };
            }
        }
    }
}