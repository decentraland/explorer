using DCL.Helpers;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class BuilderInWorldTestHelper 
{
    public static void CreateTestCatalogLocal()
    {
        AssetCatalogBridge.ClearCatalog();

        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/sceneObjectCatalog.json";


        if(File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            AssetCatalogBridge.i.AddFullSceneObjectCatalog(jsonValue);
        }
    }

    public static void CreateNFT()
    {     
        string jsonPath = Utils.GetTestAssetsPathRaw() + "/BuilderInWorldCatalog/nftAsset.json";


        if (File.Exists(jsonPath))
        {
            string jsonValue = File.ReadAllText(jsonPath);
            NFTOwner owner = NFTOwner.defaultNFTOwner;
            owner.assets.Add(JsonUtility.FromJson<NFTInfo>(jsonValue));
            BuilderInWorldNFTController.i.NftsFeteched(owner);
        }

    }
}
