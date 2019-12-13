using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NFTItemInfo : MonoBehaviour
{
    [Serializable]
    internal class IconToGameObjectMap
    {
        public string iconId;
        public GameObject gameObject;
    }

    public class Model
    {
        public string name;
        public string thumbnail;
        public List<string> iconIds;
        public string description;
        public int mintedAt;
        public int mintedTotal;

        public static Model FromWearableItem(WearableItem wearable)
        {
            var iconsIds = wearable.representations.SelectMany(x => x.bodyShapes).ToList();
            iconsIds.Add(wearable.category);

            return new Model()
            {
                name = wearable.GetName(),
                thumbnail = wearable.baseUrl + wearable.thumbnail,
                iconIds =  iconsIds,
                description = wearable.description,
                mintedAt = wearable.mintedAt,
                mintedTotal = wearable.mintedTotal,
            };
        }
    }

    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Image thumbnail;
    [SerializeField] private IconToGameObjectMap[] icons;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI minted;

    private Model currentModel;

    public void SetModel(Model newModel)
    {
        ThumbnailsManager.CancelRequest(currentModel?.thumbnail, UpdateItemThumbnail);
        currentModel = newModel;

        name.text = currentModel.name;
        ThumbnailsManager.RequestThumbnail(currentModel.thumbnail, UpdateItemThumbnail);
        foreach (var icon in icons)
        {
            icon.gameObject.SetActive(currentModel.iconIds.Contains(icon.iconId));
        }
        description.text = currentModel.description;
        minted.text = $"{currentModel.mintedAt} / {currentModel.mintedTotal}";
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    private void UpdateItemThumbnail(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }
}