namespace DCL
{
    /// <summary>
    /// This class group all our custom WebRequests types in a single entity to ease its use.
    /// </summary>
    public interface IWebRequestController
    {
        /// <summary>
        /// Use it for making generic requests to any url.
        /// </summary>
        IWebRequest webRequest { get; }

        /// <summary>
        /// Use it when you need to request an asset bundle.
        /// </summary>
        IWebRequestAssetBundle webRequestAB { get; }

        /// <summary>
        /// Use it when you need to request a texture.
        /// </summary>
        IWebRequestTexture webRequestTexture { get; }

        /// <summary>
        /// Use it when you need to request an audio.
        /// </summary>
        IWebRequestAudio webRequestAudio { get; }

        void Initialize(
            IWebRequest webRequest,
            IWebRequestAssetBundle webRequestAB,
            IWebRequestTexture webRequestTexture,
            IWebRequestAudio webRequestAudio);
    }

    public class WebRequestController : IWebRequestController
    {
        public IWebRequest webRequest { get; private set; }
        public IWebRequestAssetBundle webRequestAB { get; private set; }
        public IWebRequestTexture webRequestTexture { get; private set; }
        public IWebRequestAudio webRequestAudio { get; private set; }

        public void Initialize(
            IWebRequest webRequest,
            IWebRequestAssetBundle webRequestAB,
            IWebRequestTexture webRequestTexture,
            IWebRequestAudio webRequestAudio)
        {
            this.webRequest = webRequest;
            this.webRequestAB = webRequestAB;
            this.webRequestTexture = webRequestTexture;
            this.webRequestAudio = webRequestAudio;
        }
    }
}