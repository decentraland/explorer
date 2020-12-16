using DCL.Controllers;

namespace DCL
{
    public static class WorldRuntimeContextFactory
    {
        public static WorldRuntimeContext CreateDefault()
        {
            return new WorldRuntimeContext(
                worldState: new WorldState(),
                sceneController: new SceneController(),
                pointerEventsController: new PointerEventsController(),
                sceneBoundsChecker: new SceneBoundsChecker(),
                worldBlockersController: new WorldBlockersController());
        }
    }
}