using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace DCL
{
    public sealed partial class MockWrappers
    {
        public class DownloadHandler_Mock : DownloadHandlerScript
        {
            public string mockedText;
            public byte[] mockedData;

            protected override string GetText()
            {
                return mockedText;
            }

            protected override byte[] GetData()
            {
                return mockedData;
            }
        }

        public class WebRequest : IWebRequest
        {
            //This field maps url to url contents.
            public Dictionary<string, string> mockedContent = new Dictionary<string, string>();
            public float mockedDownloadTime = 0;

            public DownloadHandler Get(string url)
            {
                var buffer = new DownloadHandler_Mock();
                buffer.mockedText = mockedContent[url];
                return buffer;
            }

            public void GetAsync(string url, Action<DownloadHandler> OnCompleted, Action<string> OnFail)
            {
                if (mockedContent.ContainsKey(url))
                {
                    var buffer = new DownloadHandler_Mock();
                    buffer.mockedText = mockedContent[url];
                    OnCompleted?.Invoke(buffer);
                    return;
                }

                OnFail?.Invoke("Url not found!");
            }
        }
    }
}