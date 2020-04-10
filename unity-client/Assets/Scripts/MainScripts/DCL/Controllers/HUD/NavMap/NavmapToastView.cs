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
        [SerializeField] internal RectTransform toastContainer;
        [SerializeField] internal Image scenePreviewImage;

        [SerializeField] internal Button goToButton;
        [SerializeField] internal Button closeButton;
        Vector2Int location;
        RectTransform rectTransform;

        public System.Action OnGotoClicked;

        public bool isOpen
        {
            get { return gameObject.activeInHierarchy; }
        }

        private void Awake()
        {
            rectTransform = transform as RectTransform;

            goToButton.onClick.AddListener(OnGotoClick);
            closeButton.onClick.AddListener(OnCloseClick);
        }

        public void Populate(Vector2Int coordinates, MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            bool sceneInfoExists = sceneInfo != null;

            gameObject.SetActive(true);
            location = coordinates;

            PositionToast(coordinates);

            sceneLocationText.text = $"{coordinates.x}, {coordinates.y}";

            sceneOwnerText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.owner));
            sceneDescriptionText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.description));
            sceneTitleText.transform.parent.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.name));
            scenePreviewImage.gameObject.SetActive(sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.previewImageUrl));

            if (sceneInfoExists)
            {
                sceneTitleText.text = sceneInfo.name;
                sceneOwnerText.text = $"Created by: {sceneInfo.owner}";
                sceneDescriptionText.text = sceneInfo.description;

                if (currentImageUrl == sceneInfo.previewImageUrl) return;

                if (currentImage != null)
                    Destroy(currentImage);

                if (downloadCoroutine != null)
                    CoroutineStarter.Stop(downloadCoroutine);

                if (sceneInfoExists && !string.IsNullOrEmpty(sceneInfo.previewImageUrl))
                    downloadCoroutine = CoroutineStarter.Start(Download(sceneInfo.previewImageUrl));

                currentImageUrl = sceneInfoExists ? sceneInfo.previewImageUrl : "";
            }
        }

        void PositionToast(Vector2Int coordinates)
        {
            // position the toast over the parcel parcelHighlightImage so that we can easily check with local pos info where it is on the screen
            toastContainer.position = MapRenderer.i.parcelHighlightImage.transform.position;

            bool useBottom = toastContainer.localPosition.y > 0;
            bool useLeft = toastContainer.localPosition.x > 0;

            float offsetHorizontalMultiplier = toastContainer.rect.width * 0.5f;
            float offsetVerticalMultiplier = toastContainer.rect.height * 0.5f;
            Vector3 offset = toastContainer.up * offsetVerticalMultiplier * (useBottom ? -1 : 1) + toastContainer.right * offsetHorizontalMultiplier * (useLeft ? -1 : 1);

            toastContainer.position = MapRenderer.i.parcelHighlightImage.transform.position + offset;
        }

        public void OnCloseClick()
        {
            gameObject.SetActive(false);
        }

        private void OnGotoClick()
        {
            OnGotoClicked?.Invoke();

            WebInterface.GoTo(location.x, location.y);

            OnCloseClick();
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
