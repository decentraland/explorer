using UnityEngine;

public class CatalogController : MonoBehaviour
{
    public static CatalogController i { get; private set; }

    private static ItemDictionary itemCatalogValue;

    public static ItemDictionary itemCatalog
    {
        get
        {
            if (itemCatalogValue == null)
            {
                itemCatalogValue = Resources.Load<ItemDictionary>("ItemCatalog");
            }

            return itemCatalogValue;
        }
    }

    private static WearableDictionary wearableCatalogValue;

    public static WearableDictionary wearableCatalog
    {
        get
        {
            if (wearableCatalogValue == null)
            {
                wearableCatalogValue = Resources.Load<WearableDictionary>("WearableCatalog");
            }

            return wearableCatalogValue;
        }
    }

    public void Awake()
    {
        i = this;
    }

    public void AddWearableToCatalog(string payload)
    {
        Item item = JsonUtility.FromJson<Item>(payload);
        switch (item.type)
        {
            case "wearable":
                {
                    WearableItem wearableItem = JsonUtility.FromJson<WearableItem>(payload);
                    if (wearableItem.IsCollectible())
                    {
                        FillInfoWithContent(wearableItem);
                    }
                    wearableCatalog.Add(wearableItem.id, wearableItem);
                    break;
                }
            case "item":
                {
                    itemCatalog.Add(item.id, item);
                    break;
                }
            default:
                {
                    Debug.LogError("Bad type in item, will not be added to catalog");
                    break;
                }
        }
    }

    private void FillInfoWithContent(WearableItem wearableItem)
    {

        switch (Random.Range(0, 3))
        {
            case 0:
                wearableItem.rarity = "rare";
                break;
            case 1:
                wearableItem.rarity = "epic";
                break;
            case 2:
                wearableItem.rarity = "legendary";
                break;
        }

        wearableItem.description = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum";
        wearableItem.issuedId = Random.Range(1, 5000);

    }

    public void AddWearablesToCatalog(string payload)
    {
        Item[] items = JsonUtility.FromJson<Item[]>(payload);

        int count = items.Length;
        for (int i = 0; i < count; ++i)
        {
            itemCatalog.Add(items[i].id, items[i]);
        }
    }

    public void RemoveWearablesFromCatalog(string payload)
    {
        string[] itemIDs = JsonUtility.FromJson<string[]>(payload);

        int count = itemIDs.Length;
        for (int i = 0; i < count; ++i)
        {
            itemCatalog.Remove(itemIDs[i]);
            wearableCatalog.Remove(itemIDs[i]);
        }
    }

    public void ClearWearableCatalog()
    {
        itemCatalog?.Clear();
        wearableCatalog?.Clear();
    }
}
