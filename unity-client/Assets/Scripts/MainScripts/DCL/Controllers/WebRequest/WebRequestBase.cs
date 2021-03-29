using DCL.Helpers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Base class for all our custom WebRequest implementations that will manage the core lyfecicle of the requests, including the retry system.
    /// </summary>
    public interface IWebRequestBase
    {
        /// <summary>
        /// Download data from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnCompleted">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        UnityWebRequest Get(string url, Action<UnityWebRequest> OnCompleted, Action<string> OnFail = null, int requestAttemps = 3);
    }

    public abstract class WebRequestBase : IWebRequestBase
    {
        public UnityWebRequest Get(string url, Action<UnityWebRequest> OnCompleted, Action<string> OnFail = null, int requestAttemps = 3)
        {
            UnityWebRequest newWebRequest = CreateWebRequest(url);
            CoroutineStarter.Start(GetAsyncCoroutine(newWebRequest, OnCompleted, OnFail, requestAttemps));
            return newWebRequest;
        }

        private IEnumerator GetAsyncCoroutine(UnityWebRequest request, Action<UnityWebRequest> OnCompleted, Action<string> OnFail, int requestAttemps)
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            do
            {
                yield return request.SendWebRequest();

                remainingAttemps--;
                if (remainingAttemps == 0)
                {
                    OnFail?.Invoke(request.error);
                    yield break;
                }
            } while (!request.WebRequestSucceded());

            OnCompleted?.Invoke(request);
        }

        /// <summary>
        /// Specific implementation for each type of request
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <returns></returns>
        protected abstract UnityWebRequest CreateWebRequest(string url);
    }
}