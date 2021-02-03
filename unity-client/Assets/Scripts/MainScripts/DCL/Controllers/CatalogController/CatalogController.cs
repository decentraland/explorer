using DCL;
using DCL.Helpers;
using DCL.Interface;
using System.Collections.Generic;
using UnityEngine;

public class CatalogController : MonoBehaviour
{
    public static bool VERBOSE = false;
    private const string OWNED_WEARABLES_CONTEXT = "OwnedWearables";
    private const string BASE_WEARABLES_CONTEXT = "BaseWearables";

    public static CatalogController i { get; private set; }

    public static BaseDictionary<string, Item> itemCatalog => DataStore.Catalog.items;
    public static BaseDictionary<string, WearableItem> wearableCatalog => DataStore.Catalog.wearables;

    private static Dictionary<string, Promise<WearableItem>> pendingWearablePromises = new Dictionary<string, Promise<WearableItem>>();
    private static Dictionary<string, Promise<WearableItem[]>> pendingWearablesByContextPromises = new Dictionary<string, Promise<WearableItem[]>>();

    public void Awake()
    {
        i = this;
    }

    public void AddWearablesToCatalog(string payload)
    {
        WearablesRequestResponse request = JsonUtility.FromJson<WearablesRequestResponse>(payload);

        if (VERBOSE)
            Debug.Log("add wearables: " + payload);

        for (int i = 0; i < request.wearables.Length; i++)
        {
            switch (request.wearables[i].type)
            {
                case "wearable":
                    {
                        WearableItem wearableItem = request.wearables[i];

                        if (!wearableCatalog.ContainsKey(wearableItem.id))
                        {
                            wearableCatalog.Add(wearableItem.id, wearableItem);
                            ResolvePendingWearablePromise(wearableItem.id, wearableItem);
                        }

                        break;
                    }
                case "item":
                    {
                        if (!itemCatalog.ContainsKey(request.wearables[i].id))
                            itemCatalog.Add(request.wearables[i].id, (Item)request.wearables[i]);

                        break;
                    }
                default:
                    {
                        Debug.LogError("Bad type in item, will not be added to catalog");
                        break;
                    }
            }
        }

        if (!string.IsNullOrEmpty(request.context))
            ResolvePendingWearablesByContextPromise(request.context, request.wearables);
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
        Promise<WearableItem> promiseResult = new Promise<WearableItem>();

        if (wearableCatalog.TryGetValue(wearableId, out WearableItem wearable))
        {
            promiseResult.Resolve(wearable);
        }
        else
        {
            if (!pendingWearablePromises.ContainsKey(wearableId))
            {
                pendingWearablePromises.Add(wearableId, promiseResult);
                WebInterface.RequestWearables(
                    ownedByUser: null,
                    wearableIds: new string[] { wearableId },
                    collectionNames: null,
                    context: null
                );
            }
            else
            {
                pendingWearablePromises.TryGetValue(wearableId, out promiseResult);
            }
        }

        return promiseResult;
    }

    public static Promise<WearableItem[]> RequestOwnedWearables()
    {
        Promise<WearableItem[]> promiseResult = new Promise<WearableItem[]>();

        if (!pendingWearablesByContextPromises.ContainsKey(OWNED_WEARABLES_CONTEXT))
        {
            pendingWearablesByContextPromises.Add(OWNED_WEARABLES_CONTEXT, promiseResult);
            WebInterface.RequestWearables(
                ownedByUser: true,
                wearableIds: null,
                collectionNames: null,
                context: OWNED_WEARABLES_CONTEXT
            );
        }
        else
        {
            pendingWearablesByContextPromises.TryGetValue(OWNED_WEARABLES_CONTEXT, out promiseResult);
        }

        return promiseResult;
    }

    public static Promise<WearableItem[]> RequestBaseWearables()
    {
        Promise<WearableItem[]> promiseResult = new Promise<WearableItem[]>();

        if (!pendingWearablesByContextPromises.ContainsKey(BASE_WEARABLES_CONTEXT))
        {
            pendingWearablesByContextPromises.Add(BASE_WEARABLES_CONTEXT, promiseResult);
            WebInterface.RequestWearables(
                ownedByUser: null,
                wearableIds: null,
                collectionNames: new string[] { "base-avatars" },
                context: BASE_WEARABLES_CONTEXT
            );
        }
        else
        {
            pendingWearablesByContextPromises.TryGetValue(OWNED_WEARABLES_CONTEXT, out promiseResult);
        }

        return promiseResult;
    }

    private void ResolvePendingWearablePromise(string wearableId, WearableItem wearable)
    {
        if (pendingWearablePromises.TryGetValue(wearableId, out Promise<WearableItem> promise))
        {
            promise.Resolve(wearable);
            pendingWearablePromises.Remove(wearableId);
        }
    }

    private void ResolvePendingWearablesByContextPromise(string context, WearableItem[] wearables)
    {
        if (pendingWearablesByContextPromises.TryGetValue(context, out Promise<WearableItem[]> promise))
        {
            promise.Resolve(wearables);
            pendingWearablesByContextPromises.Remove(context);
        }
    }
}
