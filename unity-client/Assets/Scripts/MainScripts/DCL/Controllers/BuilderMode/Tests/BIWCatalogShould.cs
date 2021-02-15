using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWCatalogShould : IntegrationTestSuite_Legacy
{
    [Test]
    public void CatalogItemsSceneObject()
    {
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateTestCatalogLocal();

        Assert.Greater(DataStore.BuilderInWorld.catalogItemDict.Count(), 0);
        Assert.Greater(DataStore.BuilderInWorld.catalogItemPackDict.Count(), 0);
        Assert.Greater(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 0);

    }

    [Test]
    public void CatalogItemsNfts()
    {
        BIWCatalogManager.Init();

        BuilderInWorldTestHelper.CreateNFT();

        Assert.Greater(DataStore.BuilderInWorld.catalogItemDict.Count(), 0);
        Assert.Greater(DataStore.BuilderInWorld.catalogItemPackDict.Count(), 0);
        Assert.Greater(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories().Count, 0);
    }

    protected override IEnumerator TearDown()
    {
        AssetCatalogBridge.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        yield return base.TearDown();
    }
}
