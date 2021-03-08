using System;
using DCL.Helpers;
using UnityEngine;

public delegate void OnSearchResultDelegate (string searchInput, UserProfileModel[] profiles);

public interface IUsersSearchBridge
{
    event OnSearchResultDelegate OnSearchResult;
}

public class UsersSearchBridge : MonoBehaviour, IUsersSearchBridge
{
    public event OnSearchResultDelegate OnSearchResult;

    public static UsersSearchBridge i { get; private set; }

    void Awake()
    {
        if (i != null)
        {
            Utils.SafeDestroy(this);
            return;
        }

        i = this;
    }

    public void SetENSOwnerQueryResult(string payload)
    {
        ResultPayload result = Utils.SafeFromJson<ResultPayload>(payload);
        OnSearchResult?.Invoke(result.searchInput, result.success? result.profiles : null);
    }

    [Serializable]
    class ResultPayload
    {
        public string searchInput;
        public bool success;
        public UserProfileModel[] profiles;
    }
}
