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
        [SerializeField] private TextMeshProUGUI sceneTitleText;
        [SerializeField] private TextMeshProUGUI sceneOwnerText;
        [SerializeField] private TextMeshProUGUI sceneLocationText;
        [SerializeField] private TextMeshProUGUI sceneDescriptionText;
        [SerializeField] private Image scenePreviewImage;

        [SerializeField] private Button goToButton;
        [SerializeField] private Button closeButton;

        Vector2Int location;

        public void Populate(Vector2Int coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (sceneInfo == null)
                return;

            sceneOwnerText.gameObject.SetActive(!string.IsNullOrEmpty(sceneInfo.owner));
            sceneDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(sceneInfo.description));
            scenePreviewImage.gameObject.SetActive(!string.IsNullOrEmpty(sceneInfo.previewImageUrl));
            sceneLocationText.text = $"{coordinates.x}, {coordinates.y}";

            sceneTitleText.text = sceneInfo.name;
            sceneOwnerText.text = sceneInfo.owner;
            sceneDescriptionText.text = sceneInfo.description;

            location = coordinates;

            goToButton.onClick.AddListener(OnGotoClick);
            closeButton.onClick.AddListener(OnCloseClick);

            gameObject.SetActive(true);

            if (currentImageUrl == sceneInfo.previewImageUrl)
                return;

            if (currentImage != null)
                Destroy(currentImage);

            if (downloadCoroutine != null)
                CoroutineStarter.Stop(downloadCoroutine);

            if (!string.IsNullOrEmpty(sceneInfo.previewImageUrl))
                downloadCoroutine = CoroutineStarter.Start(Download(sceneInfo.previewImageUrl));

            currentImageUrl = sceneInfo.previewImageUrl;
        }

        private void OnCloseClick()
        {
            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
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
