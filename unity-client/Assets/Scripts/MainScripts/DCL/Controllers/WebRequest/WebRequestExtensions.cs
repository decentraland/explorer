using UnityEngine.Networking;

namespace DCL
{
    public static class WebRequestExtensions
    {
        public static bool WebRequestSucceded(this UnityWebRequest request)
        {
            return request != null &&
                   !request.isNetworkError &&
                   !request.isHttpError;
        }

        public static bool WebRequestServerError(this UnityWebRequest request)
        {
            return request != null &&
                   request.responseCode >= 500 &&
                   request.responseCode < 600;
        }

        public static bool WebRequestAborted(this UnityWebRequest request)
        {
            return request != null &&
                   request.isNetworkError &&
                   request.isHttpError &&
                   !string.IsNullOrEmpty(request.error) &&
                   request.error.ToLower().Contains("aborted");
        }
    }
}