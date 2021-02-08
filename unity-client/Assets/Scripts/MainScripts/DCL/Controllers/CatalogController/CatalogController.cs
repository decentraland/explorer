using DCL;
using DCL.Helpers;
using DCL.Interface;
using System.Collections.Generic;
using UnityEngine;
using static DCL.Interface.WebInterface;

public class CatalogController : MonoBehaviour
{
    public static bool VERBOSE = false;
    private const string OWNED_WEARABLES_CONTEXT = "OwnedWearables";
    private const string BASE_WEARABLES_CONTEXT = "BaseWearables";
    private const float REQUESTS_TIME_OUT = 5f;
    private const int FRAMES_TO_CHECK_FOR_SEND_REQUESTS = 1;

    public static CatalogController i { get; private set; }

    public static BaseDictionary<string, Item> itemCatalog => DataStore.Catalog.items;
    public static BaseDictionary<string, WearableItem> wearableCatalog => DataStore.Catalog.wearables;

    private static Dictionary<string, Promise<WearableItem>> awaitingWearablePromises = new Dictionary<string, Promise<WearableItem>>();
    private static Dictionary<string, float> pendingWearableRequestedTimes = new Dictionary<string, float>();
    private static List<string> pendingWearableRequests = new List<string>();

    private static Dictionary<string, Promise<WearableItem[]>> pendingWearablesByContextPromises = new Dictionary<string, Promise<WearableItem[]>>();
    private static Dictionary<string, float> pendingWearablesByContextRequestedTimes = new Dictionary<string, float>();

    public void Awake()
    {
        i = this;
    }

    private void Update()
    {
        if (Time.frameCount % FRAMES_TO_CHECK_FOR_SEND_REQUESTS == 0)
        {
            SendPendingRequests();
            CheckForWearableRequestsTimeOuts();
            CheckForWearablesBycontextRequestsTimeOuts();
        }
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
                            pendingWearableRequestedTimes.Remove(wearableItem.id);
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
        {
            ResolvePendingWearablesByContextPromise(request.context, request.wearables);
            pendingWearablesByContextRequestedTimes.Remove(request.context);
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
        Promise<WearableItem> promiseResult = new Promise<WearableItem>();

        if (wearableCatalog.TryGetValue(wearableId, out WearableItem wearable))
        {
            promiseResult.Resolve(wearable);
        }
        else
        {
            if (!awaitingWearablePromises.ContainsKey(wearableId))
            {
                awaitingWearablePromises.Add(wearableId, promiseResult);

                // We accumulate all the requests during the same frames interval to send them all together
                pendingWearableRequests.Add(wearableId);
            }
            else
            {
                awaitingWearablePromises.TryGetValue(wearableId, out promiseResult);
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
            pendingWearablesByContextRequestedTimes.Add(OWNED_WEARABLES_CONTEXT, Time.realtimeSinceStartup);
            WebInterface.RequestWearables(
                ownedByUser: true,
                wearableIds: null,
                collectionIds: null,
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
            pendingWearablesByContextRequestedTimes.Add(BASE_WEARABLES_CONTEXT, Time.realtimeSinceStartup);
            WebInterface.RequestWearables(
                ownedByUser: false,
                wearableIds: null,
                collectionIds: new string[] { "base-avatars" },
                context: BASE_WEARABLES_CONTEXT
            );
        }
        else
        {
            pendingWearablesByContextPromises.TryGetValue(OWNED_WEARABLES_CONTEXT, out promiseResult);
        }

        return promiseResult;
    }

    private void ResolvePendingWearablePromise(string wearableId, WearableItem newWearableAddedIntoCatalog = null, string errorMessage = "")
    {
        if (awaitingWearablePromises.TryGetValue(wearableId, out Promise<WearableItem> promise))
        {
            if (string.IsNullOrEmpty(errorMessage))
                promise.Resolve(newWearableAddedIntoCatalog);
            else
                promise.Reject(errorMessage);

            awaitingWearablePromises.Remove(wearableId);
        }
    }

    private void ResolvePendingWearablesByContextPromise(string context, WearableItem[] newWearablesAddedIntoCatalog = null, string errorMessage = "")
    {
        if (pendingWearablesByContextPromises.TryGetValue(context, out Promise<WearableItem[]> promise))
        {
            if (string.IsNullOrEmpty(errorMessage))
                promise.Resolve(newWearablesAddedIntoCatalog);
            else
                promise.Reject(errorMessage);

            pendingWearablesByContextPromises.Remove(context);
        }
    }

    private void SendPendingRequests()
    {
        if (pendingWearableRequests.Count > 0)
        {
            foreach (var request in pendingWearableRequests)
            {
                pendingWearableRequestedTimes.Add(request, Time.realtimeSinceStartup);
            }

            WebInterface.RequestWearables(
                ownedByUser: false,
                wearableIds: pendingWearableRequests.ToArray(),
                collectionIds: null,
                context: null
            );

            pendingWearableRequests.Clear();
        }
    }

    private void CheckForWearableRequestsTimeOuts()
    {
        if (pendingWearableRequestedTimes.Count > 0)
        {
            List<string> expiredRequestedTimes = new List<string>();
            foreach (var promiseRequestedTime in pendingWearableRequestedTimes)
            {
                if ((Time.realtimeSinceStartup - promiseRequestedTime.Value) > REQUESTS_TIME_OUT)
                {
                    ResolvePendingWearablePromise(
                        promiseRequestedTime.Key,
                        null,
                        $"The request for the wearable '{promiseRequestedTime.Key}' has exceed the set timeout!");
                    expiredRequestedTimes.Add(promiseRequestedTime.Key);
                }
            }

            foreach (var expiredTimeToRemove in expiredRequestedTimes)
            {
                pendingWearableRequestedTimes.Remove(expiredTimeToRemove);
            }
        }
    }

    private void CheckForWearablesBycontextRequestsTimeOuts()
    {
        if (pendingWearablesByContextRequestedTimes.Count > 0)
        {
            List<string> expiredRequestedTimes = new List<string>();
            foreach (var promiseByContextRequestedTime in pendingWearablesByContextRequestedTimes)
            {
                if ((Time.realtimeSinceStartup - promiseByContextRequestedTime.Value) > REQUESTS_TIME_OUT)
                {
                    ResolvePendingWearablesByContextPromise(
                        promiseByContextRequestedTime.Key,
                        null,
                        $"The request for the wearable context '{promiseByContextRequestedTime.Key}' has exceed the set timeout!");
                    expiredRequestedTimes.Add(promiseByContextRequestedTime.Key);
                }
            }

            foreach (var expiredTimeToRemove in expiredRequestedTimes)
            {
                pendingWearablesByContextRequestedTimes.Remove(expiredTimeToRemove);
            }
        }
    }
}
