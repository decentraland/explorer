using System;
using System.Collections;
using UnityEngine.Networking;

namespace DCL.Helpers.NFT.Markets
{
    public class OpenSea : INFTMarket
    {
        const string API_URL_SINGLE_ASSET = "https://api.opensea.io/api/v1/asset/{0}/{1}/";

        MarketInfo INFTMarket.marketInfo => openSeaMarketInfo;
        MarketInfo openSeaMarketInfo = new MarketInfo() { name = "OpenSea" };

        IEnumerator INFTMarket.fetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess)
        {
            string url = string.Format(API_URL_SINGLE_ASSET, assetContractAddress, tokenId);
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (!request.isNetworkError && !request.isHttpError)
                {
                    AssetResponse response = Utils.FromJsonWithNulls<AssetResponse>(request.downloadHandler.text);
                }
            }
        }

        private NFTInfo responseToNFTInfo(AssetResponse response)
        {
            NFTInfo ret = NFTInfo.defaultNFTInfo;
            ret.marketInfo = openSeaMarketInfo;
            ret.name = response.name;
            ret.description = response.description;
            ret.thumbnailUrl = response.image_thumbnail_url;
            ret.assetLink = response.external_link;
            ret.marketLink = response.permalink;

            if (!string.IsNullOrEmpty(response.owner?.address))
            {
                ret.owner = response.owner.Value.address;
            }

            if (response.num_sales != null)
            {
                ret.numSales = response.num_sales.Value;
            }

            if (response.last_sale != null)
            {
                ret.lastSaleDate = response.last_sale.Value.event_timestamp;

                if (response.last_sale.Value.payment_token != null)
                {
                    ret.lastSaleAmount = priceToFloatingPointString(response.last_sale.Value);
                    ret.lastSaleToken = new NFT.PaymentTokenInfo()
                    {
                        symbol = response.last_sale.Value.payment_token.Value.symbol
                    };
                }
            }

            UnityEngine.Color backgroundColor;
            if (UnityEngine.ColorUtility.TryParseHtmlString(response.background_color, out backgroundColor))
            {
                ret.backgroundColor = backgroundColor;
            }

            return ret;
        }

        private string priceToFloatingPointString(AssetSaleInfo saleInfo)
        {
            if (saleInfo.payment_token == null) return null;
            return priceToFloatingPointString(saleInfo.total_price, saleInfo.payment_token.Value);
        }

        private string priceToFloatingPointString(string price, PaymentTokenInfo tokenInfo)
        {
            return price.Insert(price.Length - tokenInfo.decimals, ".");
        }

        [Serializable]
        struct AssetResponse
        {
            public string token_id;
            public long? num_sales;
            public string background_color;
            public string image_url;
            public string image_preview_url;
            public string image_thumbnail_url;
            public string image_original_url;
            public string name;
            public string description;
            public string external_link;
            public AssetContract? asset_contract;
            public AccountInfo? owner;
            public string permalink;
            public AssetSaleInfo? last_sale;
        }

        [Serializable]
        struct AssetContract
        {
            public string address;
            public string asset_contract_type;
            public string created_date;
            public string name;
            public string nft_version;
            public long? owner;
            public string schema_name;
            public string symbol;
            public long? total_supply;
            public string description;
            public string external_link;
            public string image_url;
        }

        [Serializable]
        struct AccountInfo
        {
            public string profile_img_url;
            public string address;
        }

        [Serializable]
        struct AssetSaleInfo
        {
            public string event_type;
            public string event_timestamp;
            public string total_price;
            public PaymentTokenInfo? payment_token;
            public TransactionInfo? transaction;
        }

        [Serializable]
        struct PaymentTokenInfo
        {
            public long id;
            public string symbol;
            public string address;
            public string image_url;
            public string name;
            public int decimals;
            public string eth_price;
            public string usd_price;
        }

        [Serializable]
        struct TransactionInfo
        {
            public long id;
            public AccountInfo? from_account;
            public AccountInfo? to_account;
            public string transaction_hash;
        }
    }
}