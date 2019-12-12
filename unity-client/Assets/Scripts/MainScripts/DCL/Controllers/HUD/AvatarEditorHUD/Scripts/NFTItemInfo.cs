using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NFTItemInfo : MonoBehaviour
{
    public class Model
    {
        public string title;
        public string subtitle;
        public string thumbnail;
        public int level;
        public string creatorName;
        public string creatorSnapshotUrl;
        public float rating;
        public string description;
        public int mintedAt;
        public int mintedTotal;

        public static Model FromWearableItem(WearableItem wearable)
        {
            return new Model()
            {
                title = wearable.title,
                subtitle = wearable.subtitle,
                thumbnail = wearable.baseUrl + wearable.thumbnail,
                level = wearable.level,
                creatorName = wearable.creatorName,
                creatorSnapshotUrl = wearable.creatorSnapshotUrl,
                rating = wearable.rating,
                description = wearable.description,
                mintedAt = wearable.mintedAt,
                mintedTotal = wearable.mintedTotal,
            };
        }
    }

    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI creatorName;
    [SerializeField] private Image creatorSnapshot;
    [SerializeField] private TextMeshProUGUI rating;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI minted;

    private Model currentModel;

    public void SetModel(Model newModel)
    {
        ThumbnailsManager.CancelRequest(currentModel?.creatorSnapshotUrl, UpdateCreatorSprite);
        ThumbnailsManager.CancelRequest(currentModel?.thumbnail, UpdateItemThumbnail);
        currentModel = newModel;

        title.text = currentModel.title;
        subtitle.text = currentModel.subtitle;
        ThumbnailsManager.RequestThumbnail(currentModel.thumbnail, UpdateItemThumbnail);
        level.text = $"LVL {currentModel.level}";
        creatorName.text = currentModel.creatorName;
        ThumbnailsManager.RequestThumbnail(currentModel.creatorSnapshotUrl, UpdateCreatorSprite);
        rating.text = currentModel.rating.ToString("0.00");
        description.text = currentModel.description;
        minted.text = $"{currentModel.mintedAt} / {currentModel.mintedTotal}";
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    private void UpdateCreatorSprite(Sprite sprite)
    {
        creatorSnapshot.sprite = sprite;
    }

    private void UpdateItemThumbnail(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }
}