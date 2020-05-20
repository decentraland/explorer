using System;
using System.Collections;
using DCL.Helpers.NFT.Markets;

namespace DCL.Helpers.NFT
{
    public static class NFTHelper
    {
        static INFTMarket market = new OpenSea();

        static public IEnumerator fetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess)
        {
            INFTMarket selectedMarket = null;
            yield return getMarket(assetContractAddress, tokenId, (mkt) => selectedMarket = mkt);

            if (selectedMarket != null)
            {
                yield return selectedMarket.fetchNFTInfo(assetContractAddress, tokenId, onSuccess);
            }
        }

        // NOtE: this method doesn't make sense now, but it will when support for other market is added
        static public IEnumerator getMarket(string assetContractAddress, string tokenId, Action<INFTMarket> onSuccess)
        {
            onSuccess?.Invoke(market);
            yield break;
        }
    }
}