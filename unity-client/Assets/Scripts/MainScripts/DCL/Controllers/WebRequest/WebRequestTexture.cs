using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestTexture.
    /// </summary>
    public interface IWebRequestTexture : IWebRequestBase { }

    public class WebRequestTexture : WebRequestBase, IWebRequestTexture
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestTexture.GetTexture(url); }
    }
}