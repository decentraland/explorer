using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle.
    /// </summary>
    public interface IWebRequestAssetBundle : IWebRequestBase { }

    public class WebRequestAssetBundle : WebRequestBase, IWebRequestAssetBundle
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestAssetBundle.GetAssetBundle(url); }
    }
}