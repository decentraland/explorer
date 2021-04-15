using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DCL;
using DCL.Helpers;
using Variables.RealmsInfo;

public interface ICatalyst : IDisposable
{
    Promise<SceneDeploymentPayload> GetDeployedScenes(string[] parcels, bool onlyCurrentlyPointed = true, string sortBy = null, string sortOrder = null);
    Promise<string> GetDeployments(DeploymentOptions deploymentOptions);
    Promise<string> Get(string url);
}

public class Catalyst : ICatalyst
{
    private const int CACHE_TIME_MSECS = 5 * 60 * 1000;

    internal string realmDomain;

    private readonly Dictionary<string, string> cache = new Dictionary<string, string>();
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public Catalyst()
    {
        realmDomain = DataStore.i.playerRealm.Get()?.domain;
        DataStore.i.playerRealm.OnChange += PlayerRealmOnOnChange;
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= PlayerRealmOnOnChange;
        cancellationTokenSource.Cancel();
    }

    public Promise<SceneDeploymentPayload> GetDeployedScenes(string[] parcels, bool onlyCurrentlyPointed = true, string sortBy = null, string sortOrder = null)
    {
        var promise = new Promise<SceneDeploymentPayload>();

        GetDeployments(new DeploymentOptions()
            {
                filters = new DeploymentFilters()
                {
                    pointers = parcels,
                    onlyCurrentlyPointed = onlyCurrentlyPointed,
                    entityTypes = new [] { CatalystEntitiesType.SCENE }
                },
                sortBy = sortBy,
                sortOrder = sortOrder
            })
            .Then(json =>
            {
                try
                {
                    var parsedValue = Utils.SafeFromJson<SceneDeploymentPayload>(json);
                    promise.Resolve(parsedValue);
                }
                catch (Exception e)
                {
                    promise.Reject(e.Message);
                }
            })
            .Catch(error => promise.Reject(error));

        return promise;
    }

    public Promise<string> GetDeployments(DeploymentOptions deploymentOptions)
    {
        const string deploymentsUrl = "content/deployments";
        string url = $"{realmDomain}/{deploymentsUrl}/?{CatalystHelper.ToUrlParam(deploymentOptions)}";

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

        WebRequestController.i.Get(url, request =>
        {
            AddToCache(url, request.downloadHandler.text);
            promise.Resolve(request.downloadHandler.text);
        }, error =>
        {
            promise.Reject($"{error} at url {url}");
        });

        return promise;
    }

    private void PlayerRealmOnOnChange(CurrentRealmModel current, CurrentRealmModel previous)
    {
        realmDomain = current.domain;
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