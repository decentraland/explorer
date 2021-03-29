using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequest.
    /// </summary>
    public interface IWebRequest : IWebRequestBase { }

    public class WebRequest : WebRequestBase, IWebRequest
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequest.Get(url); }
    }
}