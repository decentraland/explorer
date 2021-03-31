using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// This class manage all our custom WebRequests types.
    /// </summary>
    public interface IWebRequestController
    {
        /// <summary>
        /// Initialize the controller with all the request types injected.
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="assetBundle"></param>
        /// <param name="texture"></param>
        /// <param name="audio"></param>
        void Initialize(IWebRequest generic, IWebRequest assetBundle, IWebRequest texture, IWebRequestAudio audio);

        /// <summary>
        /// Download data from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        UnityWebRequestAsyncOperation Get(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3);

        /// <summary>
        /// Download an Asset Bundle from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        UnityWebRequestAsyncOperation GetAssetBundle(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3);

        /// <summary>
        /// Download a texture from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        UnityWebRequestAsyncOperation GetTexture(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3);

        /// <summary>
        /// Download an audio clip from a url.
        /// </summary>
        /// <param name="url">Url where to make the request.</param>
        /// <param name="audioType">Type of audio that will be requested.</param>
        /// <param name="OnSuccess">This action will be executed if the request successfully finishes and it includes the request with the data downloaded.</param>
        /// <param name="OnFail">This action will be executed if the request fails.</param>
        /// <param name="requestAttemps">Number of attemps for re-trying failed requests.</param>
        UnityWebRequestAsyncOperation GetAudioClip(string url, AudioType audioType, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3);

        /// <summary>
        /// Abort and clean all the ongoing web requests.
        /// </summary>
        void Dispose();
    }

    public class WebRequestController : IWebRequestController
    {
        public static WebRequestController i { get; private set; }

        private IWebRequest genericWebRequest;
        private IWebRequest assetBundleWebRequest;
        private IWebRequest textureWebRequest;
        private IWebRequestAudio audioClipWebRequest;
        private List<UnityWebRequest> ongoingWebRequests = new List<UnityWebRequest>();

        public static WebRequestController Create()
        {
            WebRequestController newWebRequestController = new WebRequestController();

            newWebRequestController.Initialize(
                new WebRequest(),
                new WebRequestAssetBundle(),
                new WebRequestTexture(),
                new WebRequestAudio());

            return newWebRequestController;
        }

        public void Initialize(
            IWebRequest genericWebRequest,
            IWebRequest assetBundleWebRequest,
            IWebRequest textureWebRequest,
            IWebRequestAudio audioClipWebRequest)
        {
            i = this;

            this.genericWebRequest = genericWebRequest;
            this.assetBundleWebRequest = assetBundleWebRequest;
            this.textureWebRequest = textureWebRequest;
            this.audioClipWebRequest = audioClipWebRequest;
        }

        public UnityWebRequestAsyncOperation Get(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3) { return SendWebRequest(genericWebRequest, url, OnSuccess, OnFail, requestAttemps); }

        public UnityWebRequestAsyncOperation GetAssetBundle(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3) { return SendWebRequest(assetBundleWebRequest, url, OnSuccess, OnFail, requestAttemps); }

        public UnityWebRequestAsyncOperation GetTexture(string url, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3) { return SendWebRequest(textureWebRequest, url, OnSuccess, OnFail, requestAttemps); }

        public UnityWebRequestAsyncOperation GetAudioClip(string url, AudioType audioType, Action<UnityWebRequest> OnSuccess = null, Action<string> OnFail = null, int requestAttemps = 3)
        {
            audioClipWebRequest.SetAudioType(audioType);
            return SendWebRequest(audioClipWebRequest, url, OnSuccess, OnFail, requestAttemps);
        }

        private UnityWebRequestAsyncOperation SendWebRequest<T>(T requestType, string url, Action<UnityWebRequest> OnSuccess, Action<string> OnFail, int requestAttemps)
            where T : IWebRequest
        {
            int remainingAttemps = Mathf.Clamp(requestAttemps, 1, requestAttemps);

            UnityWebRequest request = requestType.CreateWebRequest(url);
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
                        SendWebRequest(requestType, url, OnSuccess, OnFail, remainingAttemps);
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

        public void Dispose()
        {
            foreach (var webRequest in ongoingWebRequests)
            {
                webRequest.Abort();
                webRequest.Dispose();
            }

            ongoingWebRequests.Clear();
        }
    }
}