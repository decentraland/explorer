using DCL.Rendering;
using NSubstitute;

namespace DCL.Tests
{
    public static class PlatformContextFactory
    {
        public static PlatformContext CreateMocked() { return CreateWithCustomMocks(); }

        public static PlatformContext CreateWithCustomMocks(
            IMemoryManager memoryManager = null,
            ICullingController cullingController = null,
            IClipboard clipboard = null,
            IPhysicsSyncController physicsSyncController = null,
            IParcelScenesCleaner parcelScenesCleaner = null,
            IDebugController debugController = null,
            IWebRequestController webRequestController = null)
        {
            IWebRequestController webRequestControllerMock = webRequestController;
            if (webRequestControllerMock == null)
            {
                webRequestControllerMock = Substitute.For<IWebRequestController>();
                webRequestControllerMock.Initialize(
                    Substitute.For<IWebRequest>(),
                    Substitute.For<IWebRequest>(),
                    Substitute.For<IWebRequest>(),
                    Substitute.For<IWebRequestAudio>());
            }

            return new PlatformContext(
                memoryManager: memoryManager ?? Substitute.For<IMemoryManager>(),
                cullingController: cullingController ?? Substitute.For<ICullingController>(),
                clipboard: clipboard ?? Substitute.For<IClipboard>(),
                physicsSyncController: physicsSyncController ?? Substitute.For<IPhysicsSyncController>(),
                parcelScenesCleaner: parcelScenesCleaner ?? Substitute.For<ParcelScenesCleaner>(),
                debugController: debugController ?? Substitute.For<IDebugController>(),
                webRequestController: webRequestControllerMock);
        }
    }
}