using DCL;
using DCL.Configuration;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BIWCatalogManager
{
    public static bool VERBOSE = false;
    private static bool IS_INIT = false;

    public static void Init()
    {
        if (!IS_INIT)
        {
            BuilderInWorldNFTController.i.OnNftsFetched += ConvertCollectiblesPack;
            AssetCatalogBridge.OnSceneObjectAdded += AddSceneObject;
            AssetCatalogBridge.OnSceneAssetPackAdded += AddSceneAssetPack;
            IS_INIT = true;
        }
    }

    public static void ClearCatalog()
    {
        DataStore.BuilderInWorld.catalogItemDict.Clear();
        DataStore.BuilderInWorld.catalogItemPackDict.Clear();
        DataStore.BuilderInWorld.catalogItemCategoryDict.Clear();
    }

    public static List<CatalogItemPack> GetCatalogItemPackList()
    {
        return DataStore.BuilderInWorld.catalogItemPackDict.GetValues();
    }

    public static List<CatalogItemPack> GetCatalogItemPacksFilteredByCategories()
    {
        var assetPacks = DataStore.BuilderInWorld.catalogItemPackDict.GetValues();

        Dictionary<string, CatalogItemPack> assetPackDic = new Dictionary<string, CatalogItemPack>();

        foreach (CatalogItemPack catalogAssetPack in assetPacks)
        {
            foreach (CatalogItem catalogItem in catalogAssetPack.assets)
            {
                if (!assetPackDic.ContainsKey(catalogItem.category))
                {
                    CatalogItemPack categoryAssetPack = new CatalogItemPack();
                    categoryAssetPack.SetThumbnailULR(catalogItem.thumbnailURL);
                    categoryAssetPack.title = catalogItem.category;
                    categoryAssetPack.assets = new List<CatalogItem>();
                    catalogItem.categoryName = catalogAssetPack.title;
                    categoryAssetPack.assets.Add(catalogItem);

                    if (!string.IsNullOrEmpty(categoryAssetPack.title))
                    {
                        if (categoryAssetPack.title.Length == 1)
                            categoryAssetPack.title = categoryAssetPack.title.ToUpper();
                        else
                            categoryAssetPack.title = char.ToUpper(categoryAssetPack.title[0]) + categoryAssetPack.title.Substring(1);
                    }

                    assetPackDic.Add(catalogItem.category, categoryAssetPack);
                    continue;
                }
                else
                {
                    catalogItem.categoryName = catalogAssetPack.title;
                    assetPackDic[catalogItem.category].assets.Add(catalogItem);
                }
            }
        }
        return assetPackDic.Values.ToList();
    }

    public static void AddSceneObject(SceneObject sceneObject)
    {
        if (DataStore.BuilderInWorld.catalogItemPackDict.ContainsKey(sceneObject.id))
            return;

        CatalogItem catalogItem = CreateCatalogItem(sceneObject);
        DataStore.BuilderInWorld.catalogItemDict.Add(catalogItem.id, catalogItem);
    }

    public static void AddSceneAssetPack(SceneAssetPack sceneAssetPack)
    {
        if (DataStore.BuilderInWorld.catalogItemPackDict.ContainsKey(sceneAssetPack.id))
            return;

        CatalogItemPack catalogItemPack = CreateCatalogItemPack(sceneAssetPack);
        DataStore.BuilderInWorld.catalogItemPackDict.Add(catalogItemPack.id, catalogItemPack);
    }

    public static void ConvertCollectiblesPack(List<NFTInfo> nftList)
    {
        if (nftList == null)
            return;

        CatalogItemPack collectiblesItemPack;

        if (!DataStore.BuilderInWorld.catalogItemPackDict.ContainsKey(BuilderInWorldSettings.ASSETS_COLLECTIBLES))
        {
            collectiblesItemPack = new CatalogItemPack();
            collectiblesItemPack.id = BuilderInWorldSettings.ASSETS_COLLECTIBLES;
            collectiblesItemPack.title = BuilderInWorldSettings.ASSETS_COLLECTIBLES;
            collectiblesItemPack.assets = new List<CatalogItem>();

            DataStore.BuilderInWorld.catalogItemPackDict.Add(collectiblesItemPack.id, collectiblesItemPack);
        }
        else
        {
            collectiblesItemPack = DataStore.BuilderInWorld.catalogItemPackDict[BuilderInWorldSettings.ASSETS_COLLECTIBLES];
            foreach (CatalogItem catalogItem in collectiblesItemPack.assets)
            {
                if (DataStore.BuilderInWorld.catalogItemDict.ContainsKey(catalogItem.id))
                    DataStore.BuilderInWorld.catalogItemDict.Remove(catalogItem.id);
            }
            collectiblesItemPack.assets.Clear();
        }

        foreach (NFTInfo info in nftList)
        {
            CatalogItem catalogItem = CreateCatalogItem(info);
            if(!DataStore.BuilderInWorld.catalogItemDict.ContainsKey(catalogItem.id))
                DataStore.BuilderInWorld.catalogItemDict.Add(catalogItem.id, catalogItem);

            collectiblesItemPack.assets.Add(catalogItem);
        }
    }

    public static CatalogItemPack CreateCatalogItemPack(SceneAssetPack sceneAssetPack)
    {
        CatalogItemPack catalogItemPack = new CatalogItemPack();

        catalogItemPack.id = sceneAssetPack.id;
        catalogItemPack.title = sceneAssetPack.title;

        catalogItemPack.assets = new List<CatalogItem>();

        catalogItemPack.SetThumbnailULR(sceneAssetPack.ComposeThumbnailUrl());

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            catalogItemPack.assets.Add(CreateCatalogItem(sceneObject));
        }

        return catalogItemPack;
    }

    public static CatalogItem CreateCatalogItem(SceneObject sceneObject)
    {
        CatalogItem catalogItem = new CatalogItem();
        catalogItem.id = sceneObject.id;
        if (sceneObject.asset_pack_id == BuilderInWorldSettings.VOXEL_ASSETS_PACK_ID)
            catalogItem.isVoxel = true;
        catalogItem.name = sceneObject.name;
        catalogItem.model = sceneObject.model;
        catalogItem.thumbnailURL = sceneObject.GetComposedThumbnailUrl();
        catalogItem.tags = sceneObject.tags;

        catalogItem.category = sceneObject.category;
        catalogItem.categoryName = catalogItem.category;

        catalogItem.contents = sceneObject.contents;

        catalogItem.metrics = sceneObject.metrics;

        if (!string.IsNullOrEmpty(sceneObject.script))
        {
            catalogItem.itemType = CatalogItem.ItemType.SMART_ITEM;
            catalogItem.parameters = sceneObject.parameters;
            catalogItem.actions = sceneObject.actions;
        }
        else
        {
            catalogItem.itemType = CatalogItem.ItemType.SCENE_OBJECT;
        }

        return catalogItem;
    }

    public static CatalogItem CreateCatalogItem(NFTInfo nFTInfo)
    {
        CatalogItem catalogItem = new CatalogItem();
        catalogItem.itemType = CatalogItem.ItemType.NFT;

        catalogItem.id = nFTInfo.assetContract.address;
        catalogItem.thumbnailURL = nFTInfo.thumbnailUrl;
        catalogItem.name = nFTInfo.name;
        catalogItem.category = nFTInfo.assetContract.name;
        catalogItem.model = $"{BuilderInWorldSettings.COLLECTIBLE_MODEL_PROTOCOL}{nFTInfo.assetContract.address}/{nFTInfo.tokenId}";
        catalogItem.tags = new List<string>();
        catalogItem.contents = new Dictionary<string, string>();
        catalogItem.metrics = new SceneObject.ObjectMetrics();

        return catalogItem;
    }
}
