using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle, including a request retry system.
    /// </summary>
    public class WebRequestAssetBundle : WebRequest
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestAssetBundle.GetAssetBundle(url); }
    }
}