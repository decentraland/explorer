using DCL.Rendering;

namespace DCL
{
    public static class PlatformContextFactory
    {
        public static PlatformContext CreateDefault()
        {
            WebRequestController newWebRequestController = new WebRequestController();
            newWebRequestController.Initialize(
                new WebRequest(),
                new WebRequestAssetBundle(),
                new WebRequestTexture(),
                new WebRequestAudio());

            return new PlatformContext(
                memoryManager: new MemoryManager(),
                cullingController: CullingController.Create(),
                clipboard: Clipboard.Create(),
                physicsSyncController: new PhysicsSyncController(),
                parcelScenesCleaner: new ParcelScenesCleaner(),
                debugController: new DebugController(),
                webRequestController: newWebRequestController);
        }
    }
}