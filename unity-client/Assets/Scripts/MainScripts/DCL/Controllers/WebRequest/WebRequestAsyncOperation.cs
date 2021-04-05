using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom request async operation to be used with the WebRequestController.
    /// </summary>
    public class WebRequestAsyncOperation : CustomYieldInstruction
    {
        public UnityWebRequest webRequest { get; }

        public bool isDone { get; private set; }

        public override bool keepWaiting { get { return webRequest != null && !webRequest.isDone; } }

        public event Action<WebRequestAsyncOperation> completed;

        public WebRequestAsyncOperation(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
            isDone = false;
        }

        internal void SetAsCompleted()
        {
            isDone = true;
            completed?.Invoke(this);
        }

        public void Abort() { webRequest?.Abort(); }
    }
}