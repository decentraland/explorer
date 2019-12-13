using System.Collections;
using System.Collections.Generic;
using AvatarShape_Tests;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarEditorHUD_Tests
{
    public class AvatarEditorHUDViewShould : TestsBase
    {
        private UserProfile userProfile;
        private AvatarEditorHUDController_Mock controller;
        private WearableDictionary catalog;

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            yield return InitScene();

            userProfile = ScriptableObject.CreateInstance<UserProfile>();
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }

            });

            catalog = AvatarTestHelpers.CreateTestCatalog();
            controller = new AvatarEditorHUDController_Mock(userProfile, catalog);
        }


        [Test]
        [TestCase("dcl://base-avatars/f_african_leggins", WearableLiterals.BodyShapes.FEMALE)]
        [TestCase("dcl://base-avatars/f_mouth_00", WearableLiterals.BodyShapes.FEMALE)]
        [TestCase("dcl://base-avatars/blue_bandana", WearableLiterals.BodyShapes.FEMALE)]
        [TestCase("dcl://base-avatars/eyebrows_02", WearableLiterals.BodyShapes.MALE)]
        [TestCase("dcl://base-avatars/m_mountainshoes.glb", WearableLiterals.BodyShapes.MALE)]
        [TestCase("dcl://base-avatars/moptop", WearableLiterals.BodyShapes.MALE)]
        public void Activate_CompatibleWithBodyShape_ItemToggle(string wearableId, string bodyShape)
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = bodyShape,
                    wearables = new List<string>() { },
                }

            });
            var category = catalog.Get(wearableId).category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            Assert.IsTrue(selector.itemToggles.ContainsKey(wearableId));
            var itemToggle = selector.itemToggles[wearableId];
            Assert.NotNull(itemToggle.wearableItem);

            Assert.AreEqual(wearableId, itemToggle.wearableItem.id);
            Assert.IsTrue(itemToggle.gameObject.activeSelf);
        }

        [Test]
        [TestCase("dcl://base-avatars/f_african_leggins", WearableLiterals.BodyShapes.MALE)]
        [TestCase("dcl://base-avatars/f_mouth_00", WearableLiterals.BodyShapes.MALE)]
        [TestCase("dcl://base-avatars/bee_t_shirt", WearableLiterals.BodyShapes.MALE)]
        [TestCase("dcl://base-avatars/eyebrows_02", WearableLiterals.BodyShapes.FEMALE)]
        [TestCase("dcl://base-avatars/m_mountainshoes.glb", WearableLiterals.BodyShapes.FEMALE)]
        [TestCase("dcl://base-avatars/moptop", WearableLiterals.BodyShapes.FEMALE)]
        public void NotCreate_IncompatibleWithBodyShape_ItemToggle(string wearableId, string bodyShape)
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = bodyShape,
                    wearables = new List<string>() { },
                }

            });
            var category = catalog.Get(wearableId).category;

            Assert.IsTrue(controller.myView.selectorsByCategory.ContainsKey(category));
            var selector = controller.myView.selectorsByCategory[category];

            Assert.IsTrue(selector.itemToggles.ContainsKey(wearableId));
            var itemToggle = selector.itemToggles[wearableId];
            Assert.NotNull(itemToggle.wearableItem);

            Assert.AreEqual(wearableId, itemToggle.wearableItem.id);
            Assert.IsFalse(itemToggle.gameObject.activeSelf);
        }

        [Test]
        [TestCase("dcl://base-avatars/f_mouth_00")]
        [TestCase("dcl://base-avatars/bee_t_shirt")]
        [TestCase("dcl://base-avatars/m_mountainshoes.glb")]
        [TestCase("dcl://base-avatars/moptop")]
        public void NotAdd_BaseWearables_ToCollectibles(string wearableId)
        {
            Assert.IsFalse(controller.myView.collectiblesItemSelector.itemToggles.ContainsKey(wearableId));
        }

        [Test]
        [TestCase("dcl://halloween_2019/zombie_suit_upper_body")]
        [TestCase("dcl://halloween_2019/vampire_upper_body")]
        [TestCase("dcl://halloween_2019/sad_clown_upper_body")]
        public void Add_Exclusives_ToCollectibles(string wearableId)
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                },
                inventory = new []{ wearableId}
            });

            Assert.IsTrue(controller.myView.collectiblesItemSelector.itemToggles.ContainsKey(wearableId));
        }

        [Test]
        [TestCase(WearableLiterals.NFTRarity.RARE)]
        [TestCase(WearableLiterals.NFTRarity.EPIC)]
        [TestCase(WearableLiterals.NFTRarity.LEGENDARY)]
        public void CreateNFTsButtonsByRarityCorrectly(string rarity)
        {
            WearableItem dummyItem = CreateDummyNFT(rarity);

            var selector = controller.myView.selectorsByCategory[dummyItem.category];
            var itemToggleObject = selector.itemToggles[dummyItem.id].gameObject;

            var originalName = selector.itemToggleFactory.nftDictionary[rarity].prefab.name;

            Assert.IsTrue(itemToggleObject.name.Contains(originalName)); //Comparing names because PrefabUtility.GetOutermostPrefabInstanceRoot(itemToggleObject) is returning null
        }

        [Test]
        public void FillNFTInfoPanelCorrectly()
        {
            WearableItem dummyItem = CreateDummyNFT(WearableLiterals.NFTRarity.EPIC);

            var itemToggle = controller.myView.selectorsByCategory[dummyItem.category].itemToggles[dummyItem.id];
            var nftInfo = (itemToggle as NFTItemToggle)?.nftItemInfo;

            Assert.NotNull(nftInfo);
            Assert.AreEqual(dummyItem.GetName(), nftInfo.name.text);
            Assert.AreEqual(dummyItem.description, nftInfo.description.text);
            Assert.AreEqual($"{dummyItem.mintedAt} / {dummyItem.mintedTotal}", nftInfo.minted.text);
        }

        private WearableItem CreateDummyNFT(string rarity)
        {
            var dummyItem = new WearableItem()
            {
                id = "dummyItem",
                rarity = rarity,
                category = WearableLiterals.Categories.EYES,
                description = "My Description",
                mintedAt = 1,
                mintedTotal = 2,
                representations = new[]
                {
                    new WearableItem.Representation()
                    {
                        bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE, WearableLiterals.BodyShapes.MALE },
                    }
                },
                tags = new [] { WearableLiterals.Tags.EXCLUSIVE },
                i18n = new [] { new i18n() { code = "en", text = "Dummy Item" } }
            };
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name", email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                },
                inventory = new [] { dummyItem.id }
            });

            catalog.Add(dummyItem.id, dummyItem);
            return dummyItem;
        }
    }
}