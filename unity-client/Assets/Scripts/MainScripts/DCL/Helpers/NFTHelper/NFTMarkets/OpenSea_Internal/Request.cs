using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0162

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    internal class RequestController
    {
        internal const bool VERBOSE = false;

        List<RequestGroup> requestGroup = new List<RequestGroup>();

        public Request AddRequest(string assetContractAddress, string tokenId)
        {
            RequestGroup group = null;
            for (int i = 0; i < requestGroup.Count; i++)
            {
                if (requestGroup[i].isOpen)
                {
                    group = requestGroup[i];
                    break;
                }
            }
            if (group == null)
            {
                group = CreateNewGroup();
            }
            return group.AddRequest(assetContractAddress, tokenId);
        }

        RequestGroup CreateNewGroup()
        {
            if (VERBOSE) Debug.Log("RequestGroup created");
            RequestGroup group = new RequestGroup(OnGroupClosed);
            requestGroup.Add(group);
            return group;
        }

        void OnGroupClosed(RequestGroup group)
        {
            if (VERBOSE) Debug.Log("RequestGroup closed");
            if (requestGroup.Contains(group))
            {
                requestGroup.Remove(group);
            }
        }
    }

    class RequestGroup : IDisposable
    {
        const string API_URL_ASSETS = "https://api.opensea.io/api/v1/assets?";
        const float WAIT_TIME = 0.2f;
        const float URL_PARAMS_MAX_LENGTH = 1854; // maxUrl(2048) - apiUrl(37) - longestPossibleRequest (78 tokenId + 42 contractAddress + 37 urlParams)

        public bool isOpen { private set; get; }

        Dictionary<string, Request> requests = new Dictionary<string, Request>();
        string requestUrl = "";

        Coroutine fetchRoutine = null;
        Action<RequestGroup> onGroupClosed = null;

        public RequestGroup(Action<RequestGroup> onGroupClosed)
        {
            isOpen = true;
            this.onGroupClosed = onGroupClosed;
            fetchRoutine = CoroutineStarter.Start(Fetch());
        }

        public Request AddRequest(string assetContractAddress, string tokenId)
        {
            string nftId = $"{assetContractAddress}/{tokenId}";

            Request request = null;
            if (requests.TryGetValue(nftId, out request))
            {
                return request;
            }

            request = new Request(assetContractAddress, tokenId);
            requests.Add(nftId, request);
            requestUrl += request.ToString();

            if (requestUrl.Length >= URL_PARAMS_MAX_LENGTH)
            {
                CloseGroup();
            }

            return request;
        }

        IEnumerator Fetch()
        {
            yield return new WaitForSeconds(WAIT_TIME);
            CloseGroup();

            string url = API_URL_ASSETS + requestUrl;

            if (RequestController.VERBOSE) Debug.Log($"Request to OpenSea {url}");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                AssetsResponse response = null;

                if (!request.isNetworkError && !request.isHttpError)
                {
                    response = Utils.FromJsonWithNulls<AssetsResponse>(request.downloadHandler.text);
                }

                using (var iterator = requests.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        if (response != null)
                            iterator.Current.Value.Resolve(response);
                        else
                            iterator.Current.Value.Resolve(request.error);
                    }
                }
            }
        }

        void CloseGroup()
        {
            isOpen = false;
            this.onGroupClosed?.Invoke(this);
            this.onGroupClosed = null;
        }

        public void Dispose()
        {
            if (fetchRoutine != null) CoroutineStarter.Stop(fetchRoutine);
            fetchRoutine = null;
            CloseGroup();
        }
    }

    internal class Request
    {
        string assetContractAddress;
        string tokenId;
        bool resolved = false;

        AssetResponse assetResponse = null;
        string error = null;

        public Request(string assetContractAddress, string tokenId)
        {
            this.assetContractAddress = assetContractAddress;
            this.tokenId = tokenId;
            if (RequestController.VERBOSE) Debug.Log($"Request created {this.ToString()}");
        }

        public void Resolve(AssetsResponse response)
        {
            AssetResponse asset = null;
            for (int i = 0; i < response.assets.Length; i++)
            {
                asset = response.assets[i];
                if (asset.token_id == tokenId && String.Equals(asset.asset_contract.address, assetContractAddress, StringComparison.OrdinalIgnoreCase))
                {
                    if (RequestController.VERBOSE) Debug.Log($"Request resolved {this.ToString()}");
                    assetResponse = asset;
                    break;
                }
            }
            if (assetResponse == null)
            {
                error = $"asset {assetContractAddress}/{tokenId} not found in api response";
                if (RequestController.VERBOSE) Debug.Log($"Request not found {JsonUtility.ToJson(response)}");
            }
            resolved = true;
        }

        public void Resolve(string error)
        {
            this.error = error;
            resolved = true;
        }

        public IEnumerator OnResolved(Action<AssetResponse> onSuccess, Action<string> onError)
        {
            yield return new WaitUntil(() => resolved);

            if (assetResponse != null)
            {
                onSuccess?.Invoke(assetResponse);
            }
            else
            {
                onError?.Invoke(error);
            }
        }

        public override string ToString()
        {
            return $"asset_contract_addresses={assetContractAddress}&token_ids={tokenId}";
        }
    }

}
