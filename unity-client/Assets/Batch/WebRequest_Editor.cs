using System;
using System.Collections;
using System.Net.Http;
using DCL.Helpers;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
using UnityGLTF;

namespace DCL
{
    public static partial class UnityEditorWrappers
    {
        public class WebRequest_Editor : IWebRequest
        {
            private static int ASSET_REQUEST_RETRY_COUNT = 5;

            public void GetAsync(string url, System.Action<DownloadHandler> OnCompleted, System.Action<string> OnFail)
            {
                EditorCoroutineUtility.StartCoroutine(GetAsyncCoroutine(url, OnCompleted, OnFail), this);
            }

            IEnumerator GetAsyncCoroutine(string url, System.Action<DownloadHandler> OnCompleted, System.Action<string> OnFail)
            {
                UnityWebRequest req;

                int retryCount = ASSET_REQUEST_RETRY_COUNT;

                do
                {
                    req = UnityWebRequest.Get(url);
                    req.SendWebRequest();

                    while (req.isDone == false)
                    {
                        yield return null;
                    }

                    retryCount--;

                    if (retryCount == 0)
                    {
                        OnFail?.Invoke(req.error);
                        yield break;
                    }
                } while (!req.WebRequestSucceded());

                OnCompleted?.Invoke(req.downloadHandler);
            }

            public DownloadHandler Get(string url)
            {
                UnityWebRequest req;

                int retryCount = ASSET_REQUEST_RETRY_COUNT;

                do
                {
                    req = UnityWebRequest.Get(url);
                    req.SendWebRequest();
                    while (req.isDone == false)
                    {
                    }

                    retryCount--;

                    if (retryCount == 0)
                    {
                        throw new HttpRequestException(req.error);
                    }
                } while (!req.WebRequestSucceded());

                return req.downloadHandler;
            }
        }
    }
}