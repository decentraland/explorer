using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public interface IWebRequestAudio
    {
        void SetAudioType(AudioType audioType);
        DownloadHandler Get(string url, int requestAttemps = 3);
        void GetAsync(string url, Action<DownloadHandler> OnCompleted, Action<string> OnFail, int requestAttemps = 3);
    }

    /// <summary>
    /// Our custom implementation of the UnityWebRequestMultimedia.GetAudioClip(), including a request retry system.
    /// </summary>
    public class WebRequestAudio : WebRequest, IWebRequestAudio
    {
        private AudioType audioType = AudioType.UNKNOWN;

        public void SetAudioType(AudioType audioType) { this.audioType = audioType; }

        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestMultimedia.GetAudioClip(url, audioType); }
    }
}