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
        IWebRequest generic { get; }

        /// <summary>
        /// Use it when you need to request an asset bundle.
        /// </summary>
        IWebRequestAssetBundle assetBundle { get; }

        /// <summary>
        /// Use it when you need to request a texture.
        /// </summary>
        IWebRequestTexture texture { get; }

        /// <summary>
        /// Use it when you need to request an audio.
        /// </summary>
        IWebRequestAudio audio { get; }

        void Initialize(
            IWebRequest webRequest,
            IWebRequestAssetBundle webRequestAB,
            IWebRequestTexture webRequestTexture,
            IWebRequestAudio webRequestAudio);
    }

    public class WebRequestController : IWebRequestController
    {
        public static WebRequestController i { get; private set; }

        public IWebRequest generic { get; private set; }
        public IWebRequestAssetBundle assetBundle { get; private set; }
        public IWebRequestTexture texture { get; private set; }
        public IWebRequestAudio audio { get; private set; }

        public static WebRequestController Create()
        {
            WebRequestController newWebRequestController = new WebRequestController();

            newWebRequestController.Initialize(
                new WebRequest(),
                new WebRequestAssetBundle(),
                new WebRequestTexture(),
                new WebRequestAudio());

            return newWebRequestController;
        }

        public void Initialize(
            IWebRequest generic,
            IWebRequestAssetBundle assetBundle,
            IWebRequestTexture texture,
            IWebRequestAudio audio)
        {
            i = this;

            this.generic = generic;
            this.assetBundle = assetBundle;
            this.texture = texture;
            this.audio = audio;
        }
    }
}