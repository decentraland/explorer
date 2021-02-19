using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BIWCatalogShould : IntegrationTestSuite_Legacy
{
    private GameObject gameObjectToUse;

    [Test]
    public void BuilderInWorldSearch()
    {
        BIWCatalogManager.Init();
        gameObjectToUse = new GameObject();
        string nameToFilter = "Sandy";
        BuilderInWorldTestHelper.CreateTestCatalogLocalMultipleFloorObjects();

        CatalogItem catalogItemToFilter = null;
        foreach (CatalogItem catalogItem in DataStore.i.builderInWorld.catalogItemDict.GetValues())
        {
            if (catalogItem.name.Contains(nameToFilter))
            {
                catalogItemToFilter = catalogItem;
                return;
            }
        }

        SceneObjectCatalogController sceneObjectCatalogController = Utils.GetOrCreateComponent<SceneObjectCatalogController>(gameObjectToUse);
        List<Dictionary<string, List<CatalogItem>>>  result = sceneObjectCatalogController.FilterAssets(nameToFilter);

        CatalogItem filteredItem =  result[0].Values.ToList()[0][0];

        Assert.AreEqual(filteredItem, catalogItemToFilter);
    }

    [Test]
    public void BuilderInWorldQuickBar()
    {
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();
        gameObjectToUse = new GameObject();
        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        CatalogItemAdapter adapter = BuilderInWorldTestHelper.CreateCatalogItemAdapter(gameObjectToUse);
        adapter.SetContent(item);

        CatalogAssetGroupAdapter groupAdatper = new CatalogAssetGroupAdapter();
        groupAdatper.AddAdapter(adapter);

        CatalogGroupListView catalogGroupListView = new CatalogGroupListView();
        catalogGroupListView.AddAdapter(groupAdatper);
        catalogGroupListView.generalCanvas = Utils.GetOrCreateComponent<Canvas>(gameObjectToUse);

        QuickBarView quickBarView = new QuickBarView();

        quickBarView.catalogGroupListView = catalogGroupListView;

        QuickBarController quickBarController = new QuickBarController(quickBarView);
        int slots = quickBarController.GetSlotsCount();
        quickBarView.shortcutsImgs = new QuickBarSlot[slots];

        for (int i = 0; i < slots; i++)
        {
            quickBarView.SetIndexToDrop(i);
            adapter.AdapterStartDragging(null);
            quickBarView.SceneObjectDropped(null);
            Assert.AreEqual(item, quickBarController.QuickBarObjectSelected(i));
        }

    }

    [Test]
    public void BuilderInWorldToggleFavorite()
    {
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();

        CatalogItem item = DataStore.i.builderInWorld.catalogItemDict.GetValues()[0];

        FavoritesController favoritesController = new FavoritesController(new CatalogGroupListView());
        favoritesController.ToggleFavoriteState(item, null);
        Assert.IsTrue(item.IsFavorite());

        favoritesController.ToggleFavoriteState(item, null);
        Assert.IsFalse(item.IsFavorite());
    }

    [Test]
    public void CatalogItemsSceneObject()
    {
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateTestCatalogLocalSingleObject();

        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);

    }

    [Test]
    public void CatalogItemsNfts()
    {
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateNFT();

        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.i.builderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);
    }

    protected override IEnumerator TearDown()
    {
        AssetCatalogBridge.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        if (gameObjectToUse != null)
            GameObject.Destroy(gameObjectToUse);
        yield return base.TearDown();
    }
}
