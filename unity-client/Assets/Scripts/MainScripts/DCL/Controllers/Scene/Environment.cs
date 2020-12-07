using DCL.Configuration;
using DCL.Controllers;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class Environment
    {
        public static readonly Environment i = new Environment();

        public SceneController sceneController;
        public DebugConfig debugConfig { get; private set; }
        public readonly WorldState worldState;
        public readonly MessagingControllersManager messagingControllersManager;
        public readonly PointerEventsController pointerEventsController;
        public readonly MemoryManager memoryManager;
        public WorldBlockersController worldBlockersController { get; private set; }
        public ICullingController cullingController { get; private set; }
        public InteractionHoverCanvasController interactionHoverCanvasController { get; private set; }

        public IParcelScenesCleaner parcelScenesCleaner { get; private set; }


        public Clipboard clipboard { get; }

        public PerformanceMetricsController performanceMetricsController { get; private set; }

        public PhysicsSyncController physicsSyncController { get; private set; }


        private bool initialized;

        private Environment()
        {
            messagingControllersManager = new MessagingControllersManager();
            pointerEventsController = new PointerEventsController();
            memoryManager = new MemoryManager();
            physicsSyncController = new PhysicsSyncController();
            performanceMetricsController = new PerformanceMetricsController();
            clipboard = Clipboard.Create();
            parcelScenesCleaner = new ParcelScenesCleaner();
            cullingController = CullingController.Create();
            worldState = new WorldState();
            debugConfig = new DebugConfig();
            sceneController = new SceneController();
        }

        public void Initialize()
        {
            if (initialized)
                return;

            messagingControllersManager.Initialize(sceneController);
            pointerEventsController.Initialize();
            memoryManager.Initialize();
            worldState.Initialize();
            worldBlockersController = WorldBlockersController.CreateWithDefaultDependencies(worldState, DCLCharacterController.i.characterPosition);
            parcelScenesCleaner.Start();
            cullingController.Start();
            debugConfig = new DebugConfig();
            sceneController.Initialize();
            initialized = true;
        }

        public void SetInteractionHoverCanvasController(InteractionHoverCanvasController controller)
        {
            interactionHoverCanvasController = controller;
        }

        public void Cleanup()
        {
            if (!initialized)
                return;

            initialized = false;

            messagingControllersManager.Cleanup();
            memoryManager.CleanupPoolsIfNeeded(true);
            pointerEventsController.Cleanup();
            worldBlockersController.Dispose();
            parcelScenesCleaner.Dispose();
            cullingController.Dispose();
        }

        public void Restart()
        {
            Cleanup();
            Initialize();
        }
    }
}