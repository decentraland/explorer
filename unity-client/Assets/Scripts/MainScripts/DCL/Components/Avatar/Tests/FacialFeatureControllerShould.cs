using System.Collections;
using DCL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = DCL.WaitUntil;

namespace AvatarShape_Tests
{
    public class FacialFeatureControllerShould : TestsBase
    {
        private const string EYES_ID = "dcl://base-avatars/f_eyes_01";
        private WearableDictionary catalog;

        //TODO: Remove it once we have a mocking library
        private class Mock_BodyShapeController : IBodyShapeController
        {
            public string bodyShapeId => WearableLiterals.BodyShapes.FEMALE;
            public void SetupEyes(Material material, Texture texture, Texture mask, Color color) { }
            public void SetupEyebrows(Material material, Texture texture, Color color) { }
            public void SetupMouth(Material material, Texture texture, Color color) { }
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            catalog = AvatarTestHelpers.CreateTestCatalogLocal();
        }

        [UnityTest]
        public IEnumerator LoadProperly()
        {
            //Arrange
            FacialFeatureController controller = new FacialFeatureController(catalog.GetOrDefault(EYES_ID), new Material(Shader.Find("DCL/Toon Shader")));

            //Act
            controller.Load(new Mock_BodyShapeController(), Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.NotNull(controller.mainTexture);
            Assert.NotNull(controller.maskTexture);
        }

        [UnityTest]
        public IEnumerator FailsGracefully_BadURL()
        {
            //Arrange
            WearableItem fakeWearable = new WearableItem
            {
                category = WearableLiterals.Categories.EYES,
                baseUrl = "http://nothing_here.nope",
                representations = new []
                {
                    new WearableItem.Representation
                    {
                        bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE },
                        contents = new [] { new ContentServerUtils.MappingPair{file = "fake.png", hash = "nope"}, new ContentServerUtils.MappingPair{file = "fake_mask.png", hash = "nope2"} },
                    }
                }
            };
            FacialFeatureController controller = new FacialFeatureController(fakeWearable, new Material(Shader.Find("DCL/Toon Shader")));

            //Act
            controller.Load(new Mock_BodyShapeController(), Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.Null(controller.mainTexture);
            Assert.Null(controller.maskTexture);
        }

        [UnityTest]
        public IEnumerator FailsGracefully_EmptyContent()
        {
            //Arrange
            WearableItem fakeWearable = new WearableItem
            {
                category = WearableLiterals.Categories.EYES,
                baseUrl = "http://nothing_here.nope",
                representations = new []
                {
                    new WearableItem.Representation
                    {
                        bodyShapes = new [] { WearableLiterals.BodyShapes.FEMALE },
                        contents = new ContentServerUtils.MappingPair[0],
                    }
                }
            };
            FacialFeatureController controller = new FacialFeatureController(fakeWearable, new Material(Shader.Find("DCL/Toon Shader")));

            //Act
            controller.Load(new Mock_BodyShapeController(), Color.red);
            yield return new WaitUntil(() => controller.isReady);

            //Assert
            Assert.Null(controller.mainTexture);
            Assert.Null(controller.maskTexture);
        }
    }
}