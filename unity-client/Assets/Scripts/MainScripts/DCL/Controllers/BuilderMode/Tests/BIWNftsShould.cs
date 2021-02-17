using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class BIWNftsShould : IntegrationTestSuite_Legacy
{

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        BIWCatalogManager.Init();
        BuilderInWorldTestHelper.CreateNFT();
    }

    [Test]
    public void NftsUsage()
    { 
        string idToTest = BuilderInWorldNFTController.i.GetNfts()[0].assetContract.address; 

        Assert.IsFalse(BuilderInWorldNFTController.i.IsNFTInUse(idToTest));

        BuilderInWorldNFTController.i.UseNFT(idToTest);

        Assert.IsTrue(BuilderInWorldNFTController.i.IsNFTInUse(idToTest));

        BuilderInWorldNFTController.i.StopUsingNFT(idToTest);

        Assert.IsFalse(BuilderInWorldNFTController.i.IsNFTInUse(idToTest));
    }

    [Test]
    public void NftComponent()
    {
        CatalogItem catalogItem = DataStore.BuilderInWorld.catalogItemDict.GetValues()[0];
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        DCLBuilderInWorldEntity biwEntity = Utils.GetOrCreateComponent<DCLBuilderInWorldEntity>(scene.entities[entityId].gameObject);
        biwEntity.Init(scene.entities[entityId], null);

        NFTShape nftShape = (NFTShape) scene.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
        nftShape.model = new NFTShape.Model();
        nftShape.model.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
        nftShape.model.src = catalogItem.model;
        nftShape.model.assetId = catalogItem.id;

        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, nftShape.id);

        Assert.IsTrue(biwEntity.IsEntityNFT());

        CatalogItem associatedCatalogItem = biwEntity.GetCatalogItemAssociated();
        Assert.IsTrue(associatedCatalogItem.IsNFT());

        Assert.AreEqual(associatedCatalogItem, catalogItem);
    }

    protected override IEnumerator TearDown()
    {
        BIWCatalogManager.ClearCatalog();
        BuilderInWorldNFTController.i.ClearNFTs();
        yield return base.TearDown();
    }
}
