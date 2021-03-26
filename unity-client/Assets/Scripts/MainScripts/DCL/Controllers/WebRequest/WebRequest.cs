using System;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequest.
    /// </summary>
    public interface IWebRequest
    {
        DownloadHandler Get(string url, int requestAttemps = 3);
        void GetAsync(string url, Action<DownloadHandler> OnCompleted, Action<string> OnFail, int requestAttemps = 3);
    }

    public class WebRequest : WebRequestBase, IWebRequest
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequest.Get(url); }
    }
}