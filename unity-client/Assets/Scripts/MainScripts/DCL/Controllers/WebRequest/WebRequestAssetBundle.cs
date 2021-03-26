using System;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestAssetBundle.
    /// </summary>
    public interface IWebRequestAssetBundle
    {
        DownloadHandler Get(string url, int requestAttemps = 3);
        void GetAsync(string url, Action<DownloadHandler> OnCompleted, Action<string> OnFail, int requestAttemps = 3);
    }

    public class WebRequestAssetBundle : WebRequestBase, IWebRequestAssetBundle
    {
        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestAssetBundle.GetAssetBundle(url); }
    }
}