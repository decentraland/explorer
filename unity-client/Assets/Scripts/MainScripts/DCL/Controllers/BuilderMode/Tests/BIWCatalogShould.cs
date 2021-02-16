using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWCatalogShould : IntegrationTestSuite_Legacy
{
    private GameObject gameObjectToUse;

    [Test]
    public void BuilderInWorldQuickBar()
    {
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocal();
        gameObjectToUse = new GameObject();
        CatalogItem item = DataStore.BuilderInWorld.catalogItemDict.GetValues()[0];

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
        BuilderInWorldTestHelper.CreateTestCatalogLocal();

        CatalogItem item = DataStore.BuilderInWorld.catalogItemDict.GetValues()[0];

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

        BuilderInWorldTestHelper.CreateTestCatalogLocal();

        Assert.AreEqual(DataStore.BuilderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.BuilderInWorld.catalogItemPackDict.Count(), 1);
        Assert.AreEqual(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 1);

    }

    [Test]
    public void CatalogItemsNfts()
    {
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateNFT();

        Assert.AreEqual(DataStore.BuilderInWorld.catalogItemDict.Count(), 1);
        Assert.AreEqual(DataStore.BuilderInWorld.catalogItemPackDict.Count(), 1);
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
