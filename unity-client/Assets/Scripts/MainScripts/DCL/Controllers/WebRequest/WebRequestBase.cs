using DCL.Helpers;
using System;
using System.Collections;
using System.Net.Http;
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
        /// Request and download data from a url (synchronous mode).
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        /// <returns>The web request with the data downloaded.</returns>
        UnityWebRequest Get(string url, int requestAttemps = 3);

        /// <summary>
        /// Download data from a url (asynchronous mode).
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnCompleted">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        void GetAsync(string url, Action<UnityWebRequest> OnCompleted, Action<string> OnFail, int requestAttemps = 3);
    }

    public abstract class WebRequestBase : IWebRequestBase
    {
        public UnityWebRequest Get(string url, int requestAttemps = 3)
        {
            UnityWebRequest request;
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            do
            {
                try
                {
                    request = CreateWebRequest(url);
                    var requestOperation = request.SendWebRequest();
                    while (!requestOperation.isDone) { }
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException($"{e.Message} -- ({url})", e);
                }

                remainingAttemps--;
                if (remainingAttemps == 0)
                {
                    throw new HttpRequestException($"{request.error} -- ({url})");
                }
            } while (!request.WebRequestSucceded());

            return request;
        }

        public void GetAsync(string url, Action<UnityWebRequest> OnCompleted, Action<string> OnFail, int requestAttemps = 3) { CoroutineStarter.Start(GetAsyncCoroutine(url, OnCompleted, OnFail, requestAttemps)); }

        private IEnumerator GetAsyncCoroutine(string url, Action<UnityWebRequest> OnCompleted, Action<string> OnFail, int requestAttemps)
        {
            UnityWebRequest request;
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            do
            {
                request = CreateWebRequest(url);
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