using System;
using System.Collections;

namespace DCL.Helpers.NFT.Markets
{
    public interface INFTMarket
    {
        IEnumerator fetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess);
    }
}