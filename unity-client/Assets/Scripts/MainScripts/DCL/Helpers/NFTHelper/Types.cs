﻿namespace DCL.Helpers.NFT
{
    public struct NFTInfo
    {
        public string name;
        public string description;
        public string owner;
        public long numSales;
        public string thumbnailUrl;
        public string assetLink;
        public string marketLink;
        public string lastSaleDate;
        public string lastSaleAmount;
        public PaymentTokenInfo lastSaleToken;
        public UnityEngine.Color backgroundColor;
        public MarketInfo marketInfo;

        static public NFTInfo defaultNFTInfo
        {
            get
            {
                NFTInfo ret = new NFTInfo();
                ret.owner = "0x0000000000000000000000000000000000000000";
                ret.backgroundColor = UnityEngine.Color.white;
                ret.numSales = 0;
                return ret;
            }
        }
    }

    public struct PaymentTokenInfo
    {
        public string symbol;
    }

    public struct MarketInfo
    {
        public string name;
    }
}