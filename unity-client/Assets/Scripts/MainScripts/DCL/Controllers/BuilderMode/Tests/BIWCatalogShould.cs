using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWCatalogShould : IntegrationTestSuite_Legacy
{
    [Test]
    public void BuilderInWorldQuickBar()
    {
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateTestCatalogLocal();

        QuickBarController quickBarController = new QuickBarController(new QuickBarView());

        for(int i = 0; i < quickBarController.GetSlotsCount();i++)
        {

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
        yield return base.TearDown();
    }
}
