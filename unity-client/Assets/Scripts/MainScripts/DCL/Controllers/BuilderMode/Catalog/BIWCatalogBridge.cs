using DCL;
using DCL.Configuration;
using DCL.Helpers.NFT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BIWCatalogBridge
{
    public static bool VERBOSE = false;
    public static AssetCatalogBridge i2 = new AssetCatalogBridge();
    public static BIWCatalogBridge i
    {
        get
        {
            if (i == null)
            {
                Init();
            }

            return i;
        }

        private set { i = value; }
    }

    static void Init()
    {
        i = new BIWCatalogBridge();

        BuilderInWorldNFTController.i.OnNftsFetched += ConvertCollectiblesPack;

        ConvertAssetsPacks();
        ConvertCollectiblesPack();
    }

    public static List<CatalogItemPack> GetCatalogItemPackList()
    {
        return DataStore.BuilderInWorld.catalogItemPackDict.GetValues();
    }

    void FilterCategories()
    {
        List<SceneAssetPack> categoryList = new List<SceneAssetPack>();
        var assetPacks = AssetCatalogBridge.sceneAssetPackCatalog.GetValues().ToList();

        Dictionary<string, SceneAssetPack> assetPackDic = new Dictionary<string, SceneAssetPack>();

        foreach (SceneAssetPack assetPack in assetPacks)
        {
            foreach (SceneObject sceneObject in assetPack.assets)
            {
                if (!assetPackDic.ContainsKey(sceneObject.category))
                {
                    SceneAssetPack categoryAssetPack = new SceneAssetPack();
                    categoryAssetPack.thumbnail = sceneObject.category;
                    categoryAssetPack.title = sceneObject.category;
                    categoryAssetPack.assets = new List<SceneObject>();
                    sceneObject.titleToShow = assetPack.title;
                    categoryAssetPack.assets.Add(sceneObject);

                    if (!string.IsNullOrEmpty(categoryAssetPack.title))
                    {
                        if (categoryAssetPack.title.Length == 1)
                            categoryAssetPack.title = categoryAssetPack.title.ToUpper();
                        else
                            categoryAssetPack.title = char.ToUpper(categoryAssetPack.title[0]) + categoryAssetPack.title.Substring(1);
                    }

                    assetPackDic.Add(sceneObject.category, categoryAssetPack);
                    continue;
                }
                else
                {
                    sceneObject.titleToShow = assetPack.title;
                    assetPackDic[sceneObject.category].assets.Add(sceneObject);
                }
            }
        }

        categoryList = assetPackDic.Values.ToList();
    }

    static void ConvertAssetsPacks()
    {
     
        foreach (SceneAssetPack sceneAssetPack in  AssetCatalogBridge.sceneAssetPackCatalog.GetValues())
        {
            CatalogItemPack catalogItemPack = CreateCatalogItemPack(sceneAssetPack);
            foreach(CatalogItem catalogItem in catalogItemPack.assets)
            {
                DataStore.BuilderInWorld.catalogItemDict.Add(catalogItem.id, catalogItem);
            }

            DataStore.BuilderInWorld.catalogItemPackDict.Add(catalogItemPack.id, catalogItemPack);
        }


    }

    static void ConvertCollectiblesPack()
    {
        CatalogItemPack collectiblesItemPack;

        if (!DataStore.BuilderInWorld.catalogItemPackDict.ContainsKey(BuilderInWorldSettings.ASSETS_COLLECTIBLES))
        {
            collectiblesItemPack = new CatalogItemPack();
            collectiblesItemPack.id = BuilderInWorldSettings.ASSETS_COLLECTIBLES;
            collectiblesItemPack.title = BuilderInWorldSettings.ASSETS_COLLECTIBLES;

            collectiblesItemPack.assets = BuilderInWorldNFTController.i.GetNFTsAsSceneObjects();

            DataStore.BuilderInWorld.catalogItemPackDict.Add(collectiblesItemPack.id, collectiblesItemPack);
        }
        else
        {
            collectiblesItemPack = DataStore.BuilderInWorld.catalogItemPackDict[BuilderInWorldSettings.ASSETS_COLLECTIBLES];
        }

        foreach (CatalogItem catalogItem in collectiblesItemPack.assets)
        {
            if(!DataStore.BuilderInWorld.catalogItemDict.ContainsKey(catalogItem.id))
                DataStore.BuilderInWorld.catalogItemDict.Add(catalogItem.id, catalogItem);
        }
    }

    private static CatalogItemPack CreateCatalogItemPack(SceneAssetPack sceneAssetPack)
    {
        CatalogItemPack catalogItemPack = new CatalogItemPack();

        catalogItemPack.id = sceneAssetPack.id;
        catalogItemPack.title = sceneAssetPack.title;

        catalogItemPack.assets = new List<CatalogItem>();

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            catalogItemPack.assets.Add(CreateCatalogItem(sceneObject));
        }

        return catalogItemPack;
    }

    private static CatalogItem CreateCatalogItem(SceneObject sceneObject)
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
            catalogItem.itemType = CatalogItem.ItemType.SMART_ITEM;
        else
            catalogItem.itemType = CatalogItem.ItemType.SCENE_OBJECT;

        return catalogItem;
    }

    private static CatalogItem CreateCatalogItem(NFTInfo nFTInfo)
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
