using DCL.Helpers;
using System;
using System.Collections.Generic;
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
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        UnityWebRequestAsyncOperation Get(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3);

        /// <summary>
        /// Abort and clean all the ongoing web requests.
        /// </summary>
        void Dispose();
    }

    public abstract class WebRequestBase : IWebRequestBase
    {
        private List<UnityWebRequest> ongoingWebRequests = new List<UnityWebRequest>();

        public UnityWebRequestAsyncOperation Get(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3)
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);
            return TrySendWebRequest(url, OnSuccess, OnFail, remainingAttemps);
        }

        private UnityWebRequestAsyncOperation TrySendWebRequest(string url, Action<UnityWebRequest> OnSuccess, Action<string> OnFail, int requestAttemps)
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = CreateWebRequest(url);
            ongoingWebRequests.Add(request);

            UnityWebRequestAsyncOperation requestOp = request.SendWebRequest();
            requestOp.completed += (asyncOp) =>
            {
                if (request.WebRequestSucceded())
                {
                    OnSuccess?.Invoke(request);
                }
                else if (!request.WebRequestAborted())
                {
                    remainingAttemps--;
                    if (remainingAttemps > 0)
                    {
                        TrySendWebRequest(url, OnSuccess, OnFail, remainingAttemps);
                    }
                    else
                    {
                        OnFail?.Invoke(request.error);
                    }
                }
                else
                {
                    OnFail?.Invoke(request.error);
                }

                ongoingWebRequests.Remove(request);
                request?.Dispose();
            };

            return requestOp;
        }

        /// <summary>
        /// Specific implementation for each type of request
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <returns></returns>
        protected abstract UnityWebRequest CreateWebRequest(string url);

        public void Dispose()
        {
            foreach (var request in ongoingWebRequests)
            {
                request.Abort();
                request.Dispose();
            }

            ongoingWebRequests.Clear();
        }
    }
}