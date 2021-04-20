using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DCL;
using DCL.Helpers;
using Variables.RealmsInfo;

public interface ICatalyst : IDisposable
{
    public string contentUrl { get; }
    Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels);
    Promise<string> GetEntities(string entityType, string[] pointers);
    Promise<string> Get(string url);
}

public class Catalyst : ICatalyst
{
    private const int CACHE_TIME_MSECS = 5 * 60 * 1000;

    public string contentUrl => realmContentServerUrl;

    private string realmDomain = "https://peer.decentraland.org";
    private string realmContentServerUrl = "https://peer.decentraland.org/content";

    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public Catalyst()
    {
        if (DataStore.i.playerRealm.Get() != null)
        {
            realmDomain = DataStore.i.playerRealm.Get().domain;
            realmContentServerUrl = DataStore.i.playerRealm.Get().contentServerUrl;
        }
        DataStore.i.playerRealm.OnChange += PlayerRealmOnOnChange;
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= PlayerRealmOnOnChange;
        cancellationTokenSource.Cancel();
    }

    public Promise<CatalystSceneEntityPayload[]> GetDeployedScenes(string[] parcels)
    {
        var promise = new Promise<CatalystSceneEntityPayload[]>();

        GetEntities(CatalystEntitiesType.SCENE, parcels)
            .Then(json =>
            {
                CatalystSceneEntityPayload[] parsedValue = null;
                bool hasException = false;
                try
                {
                    parsedValue = Utils.ParseJsonArray<CatalystSceneEntityPayload[]>(json);
                }
                catch (Exception e)
                {
                    promise.Reject(e.Message);
                    hasException = true;
                }
                finally
                {
                    if (!hasException)
                        promise.Resolve(parsedValue);
                }
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }
    
    public Promise<string> GetEntities(string entityType, string[] pointers)
    {
        string deploymentsUrl = $"content/entities/{entityType}?";
        string urlParams = "";
        urlParams = pointers.Aggregate(urlParams, (current, pointer) => current + $"&pointer={pointer}");
        
        string url = $"{realmDomain}/{deploymentsUrl}/?{urlParams}";

        return Get(url);
    }

    public Promise<string> Get(string url)
    {
        Promise<string> promise = new Promise<string>();

        if (cache.TryGetValue(url, out string cachedResult))
        {
            promise.Resolve(cachedResult);
            return promise;
        }

        WebRequestController.i.Get(url, null ,request =>
        {
            AddToCache(url, request.downloadHandler.text);
            promise.Resolve(request.downloadHandler.text);
        }, request =>
        {
            promise.Reject($"{request.error} {request.downloadHandler.text} at url {url}");
        });

        return promise;
    }

    private void PlayerRealmOnOnChange(CurrentRealmModel current, CurrentRealmModel previous)
    {
        realmDomain = current.domain;
        realmContentServerUrl = DataStore.i.playerRealm.Get().contentServerUrl;
    }

    private void AddToCache(string url, string result)
    {
        cache[url] = result;

        // NOTE: remove from cache after CACHE_TIME_MSECS time passed
        Task.Delay(CACHE_TIME_MSECS)
            .ContinueWith((task) =>
            {
                cache.Remove(url);
            }, cancellationTokenSource.Token);
    }
}