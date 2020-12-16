using DCL.Controllers;

namespace DCL
{
    public class WorldRuntimeContext : System.IDisposable
    {
        public readonly WorldState worldState;
        public readonly SceneController sceneController;
        public readonly PointerEventsController pointerEventsController;
        public readonly SceneBoundsChecker sceneBoundsChecker;
        public readonly WorldBlockersController worldBlockersController;

        public WorldRuntimeContext(WorldState worldState,
            SceneController sceneController,
            PointerEventsController pointerEventsController,
            SceneBoundsChecker sceneBoundsChecker,
            WorldBlockersController worldBlockersController)
        {
            this.worldState = worldState;
            this.sceneController = sceneController;
            this.pointerEventsController = pointerEventsController;
            this.sceneBoundsChecker = sceneBoundsChecker;
            this.worldBlockersController = worldBlockersController;
        }

        public void Dispose()
        {
            pointerEventsController.Cleanup();
            sceneBoundsChecker.Stop();
            worldBlockersController.Dispose();
            sceneController.Dispose();
        }
    }
}