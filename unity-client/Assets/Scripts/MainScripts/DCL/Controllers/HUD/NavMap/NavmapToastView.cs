using DCL.Interface;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DCL
{
    public class NavmapToastView : MonoBehaviour
    {
        [SerializeField] internal TextMeshProUGUI sceneTitleText;
        [SerializeField] internal TextMeshProUGUI sceneOwnerText;
        [SerializeField] internal TextMeshProUGUI sceneLocationText;
        [SerializeField] internal TextMeshProUGUI sceneDescriptionText;
        [SerializeField] internal Image scenePreviewImage;

        [SerializeField] internal Button goToButton;
        [SerializeField] internal Button closeButton;

        public System.Action onGotoClicked;

        Vector2Int location;

        private void Awake()
        {
            goToButton.onClick.AddListener(OnGotoClick);
            closeButton.onClick.AddListener(OnCloseClick);
        }

        public void Populate(Vector2Int coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            bool sceneInfoExtists = sceneInfo != null;

            sceneOwnerText.gameObject.SetActive(sceneInfoExtists && !string.IsNullOrEmpty(sceneInfo.owner));
            sceneDescriptionText.gameObject.SetActive(sceneInfoExtists && !string.IsNullOrEmpty(sceneInfo.description));
            scenePreviewImage.gameObject.SetActive(sceneInfoExtists && !string.IsNullOrEmpty(sceneInfo.previewImageUrl));
            sceneLocationText.text = $"{coordinates.x}, {coordinates.y}";

            sceneTitleText.text = sceneInfoExtists ? sceneInfo.name : "";
            sceneOwnerText.text = sceneInfoExtists ? $"Created by: {sceneInfo.owner}" : "";
            sceneDescriptionText.text = sceneInfoExtists ? sceneInfo.description : "";

            location = coordinates;

            gameObject.SetActive(true);

            if (sceneInfoExtists && currentImageUrl == sceneInfo.previewImageUrl)
                return;

            if (currentImage != null)
                Destroy(currentImage);

            if (downloadCoroutine != null)
                CoroutineStarter.Stop(downloadCoroutine);

            if (sceneInfoExtists && !string.IsNullOrEmpty(sceneInfo.previewImageUrl))
                downloadCoroutine = CoroutineStarter.Start(Download(sceneInfo.previewImageUrl));

            currentImageUrl = sceneInfoExtists ? sceneInfo.previewImageUrl : "";
        }

        private void OnCloseClick()
        {
            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
            onGotoClicked?.Invoke();

            WebInterface.GoTo(location.x, location.y);
        }

        string currentImageUrl;
        Texture currentImage;
        Coroutine downloadCoroutine;

        private IEnumerator Download(string url)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

            yield return www.SendWebRequest();

            Sprite sprite;

            if (!www.isNetworkError && !www.isHttpError)
            {
                var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                texture.Compress(false);
                texture.Apply(false, true);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else
            {
                Debug.Log($"Error downloading: {url} {www.error}");
                // No point on making a fancy error because this will be replaced by AssetManager. By now let's use null as fallback value.
                sprite = null;
            }

            scenePreviewImage.sprite = sprite;
            currentImage = sprite.texture;
        }
    }
}
