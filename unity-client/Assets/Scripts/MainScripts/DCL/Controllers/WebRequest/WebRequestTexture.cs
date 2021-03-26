using System;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestTexture.
    /// </summary>
    public interface IWebRequestTexture
    {
        DownloadHandler Get(string url, int requestAttemps = 3);
        void GetAsync(string url, Action<DownloadHandler> OnCompleted, Action<string> OnFail, int requestAttemps = 3);
    }

    public class WebRequestTexture : WebRequestBase, IWebRequestTexture
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestTexture.GetTexture(url); }
    }
}