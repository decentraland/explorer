namespace DCL
{
    public interface IWebRequestController
    {
        IWebRequest webRequest { get; }
        IWebRequest webRequestAB { get; }
        IWebRequest webRequestTexture { get; }
        IWebRequestAudio webRequestAudio { get; }

        void Initialize(
            IWebRequest webRequest,
            IWebRequest webRequestAB,
            IWebRequest webRequestTexture,
            IWebRequestAudio webRequestAudio);
    }

    public class WebRequestController : IWebRequestController
    {
        public IWebRequest webRequest { get; private set; }
        public IWebRequest webRequestAB { get; private set; }
        public IWebRequest webRequestTexture { get; private set; }
        public IWebRequestAudio webRequestAudio { get; private set; }

        public void Initialize(
            IWebRequest webRequest,
            IWebRequest webRequestAB,
            IWebRequest webRequestTexture,
            IWebRequestAudio webRequestAudio)
        {
            this.webRequest = webRequest;
            this.webRequestAB = webRequestAB;
            this.webRequestTexture = webRequestTexture;
            this.webRequestAudio = webRequestAudio;
        }
    }
}