using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestTexture, including a request retry system.
    /// </summary>
    public class WebRequestTexture : WebRequest
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestTexture.GetTexture(url); }
    }
}