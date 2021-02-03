using DCL;
using DCL.Helpers;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogController : MonoBehaviour
{
    public static bool VERBOSE = false;
    public static CatalogController i { get; private set; }

    public static BaseDictionary<string, Item> itemCatalog => DataStore.Catalog.items;
    public static BaseDictionary<string, WearableItem> wearableCatalog => DataStore.Catalog.wearables;

    private static Dictionary<string, Promise<WearableItem>> pendingWearablePromises = new Dictionary<string, Promise<WearableItem>>();

    public void Awake()
    {
        i = this;

        wearableCatalog.OnAdded += WearableReceivedOnCatalog;
    }

    private void OnDestroy()
    {
        wearableCatalog.OnAdded -= WearableReceivedOnCatalog;
    }

    public void AddWearableToCatalog(string payload)
    {
        WearableRequestResponse request = JsonUtility.FromJson<WearableRequestResponse>(payload);

        if (VERBOSE)
            Debug.Log("add wearable: " + payload);

        switch (request.wearable.type)
        {
            case "wearable":
                {
                    WearableItem wearableItem = request.wearable;

                    if (!wearableCatalog.ContainsKey(wearableItem.id))
                        wearableCatalog.Add(wearableItem.id, wearableItem);

                    break;
                }
            case "item":
                {
                    if (!itemCatalog.ContainsKey(request.wearable.id))
                        itemCatalog.Add(request.wearable.id, (Item)request.wearable);

                    break;
                }
            default:
                {
                    Debug.LogError("Bad type in item, will not be added to catalog");
                    break;
                }
        }
    }

    public void AddWearablesToCatalog(string payload)
    {
        Item[] items = JsonUtility.FromJson<Item[]>(payload);

        int count = items.Length;
        for (int i = 0; i < count; ++i)
        {
            if (!itemCatalog.ContainsKey(items[i].id))
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

    public static Promise<WearableItem> RequestWearable(string wearableId)
    {
        Promise<WearableItem> promise = new Promise<WearableItem>();

        if (wearableCatalog.TryGetValue(wearableId, out WearableItem wearable))
        {
            promise.Resolve(wearable);
        }
        else
        {
            if (!pendingWearablePromises.ContainsKey(wearableId))
            {
                pendingWearablePromises.Add(wearableId, promise);
                WebInterface.RequestWearables(
                    ownedByUser: null,
                    wearableIds: new string[] { wearableId },
                    collectionNames: null,
                    context: null
                );
            }
            else
            {
                pendingWearablePromises.TryGetValue(wearableId, out promise);
            }
        }

        return promise;
    }

    private void WearableReceivedOnCatalog(string wearableId, WearableItem wearable)
    {
        if (pendingWearablePromises.TryGetValue(wearableId, out Promise<WearableItem> promise))
        {
            promise.Resolve(wearable);
            pendingWearablePromises.Remove(wearableId);
        }
    }
}
