using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestMultimedia (Audio Clip).
    /// </summary>
    public interface IWebRequestAudio
    {
        /// <summary>
        /// Configure the type of the audio that will be requested.
        /// </summary>
        /// <param name="audioType">Audio type.</param>
        void SetAudioType(AudioType audioType);

        DownloadHandler Get(string url, int requestAttemps = 3);
        void GetAsync(string url, Action<DownloadHandler> OnCompleted, Action<string> OnFail, int requestAttemps = 3);
    }

    public class WebRequestAudio : WebRequestBase, IWebRequestAudio
    {
        private AudioType audioType = AudioType.UNKNOWN;

        public void SetAudioType(AudioType audioType) { this.audioType = audioType; }

        protected override UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestMultimedia.GetAudioClip(url, audioType); }
    }
}