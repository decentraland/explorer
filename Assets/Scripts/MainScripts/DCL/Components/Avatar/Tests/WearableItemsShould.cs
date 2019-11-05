using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using Tests;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AvatarShape_Tests
{
    class AvatarRenderer_Mock : AvatarRenderer
    {
        public static Dictionary<string, WearableController> GetWearableControllers(AvatarRenderer renderer)
        {
            var avatarRendererMock = new GameObject("Temp").AddComponent<AvatarRenderer_Mock>();
            avatarRendererMock.CopyFrom(renderer);

            var toReturn = avatarRendererMock.wearablesController;
            Destroy(avatarRendererMock.gameObject);

            return toReturn;
        }

        public static BodyShapeController GetBodyShape(AvatarRenderer renderer)
        {
            var avatarRendererMock = new GameObject("Temp").AddComponent<AvatarRenderer_Mock>();
            avatarRendererMock.CopyFrom(renderer);

            var toReturn = avatarRendererMock.bodyShapeController;
            Destroy(avatarRendererMock.gameObject);

            return toReturn;
        }

        protected override void OnDestroy() { }
    }

    class WearableController_Mock : WearableController
    {
        public WearableController_Mock(WearableItem wearableItem, string bodyShapeType) : base(wearableItem, bodyShapeType) { }
        public WearableController_Mock(WearableController original) : base(original) { }

        public Renderer[] myAssetRenderers => assetRenderers;
        public GameObject myAssetContainer => this.assetContainer;
    }

    class BodyShapeController_Mock : BodyShapeController
    {
        public BodyShapeController_Mock(WearableItem original) : base(original) { }
        public BodyShapeController_Mock(WearableController original) : base(original) { }

        public Renderer[] myAssetRenderers => assetRenderers;
        public GameObject myAssetContainer => this.assetContainer;
    }

    public class WearableItemsShould : TestsBase
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BLUE_BANDANA_ID = "dcl://base-avatars/blue_bandana";

        private AvatarModel avatarModel;
        private WearableDictionary catalog;
        private AvatarShape avatarShape;

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            yield return InitScene();

            avatarModel = new AvatarModel()
            {
                name = " test",
                hairColor = Color.white,
                eyeColor = Color.white,
                skinColor = Color.white,
                bodyShape = WearableLiterals.BodyShapes.FEMALE,
                wearables = new List<string>()
                {
                }
            };
            catalog = AvatarTestHelpers.CreateTestCatalog();
            avatarShape = AvatarTestHelpers.CreateAvatar(scene, avatarModel);

            yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);
        }

        [UnityTest]
        public IEnumerator BeVisibleByDefault()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID };

            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByGeneralHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.hides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByWrongGeneralHides()
        {
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.hides = new [] { "NonExistentCategory" };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.GetRepresentation(avatarModel.bodyShape).overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.GetRepresentation(WearableLiterals.BodyShapes.MALE).overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeUnequipedProperly()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            avatarModel.wearables = new List<string>() { };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            var wearableControllers = AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer);

            Assert.IsFalse(wearableControllers.ContainsKey(SUNGLASSES_ID));
        }

        [UnityTest]
        public IEnumerator BeUnequipedProperlyMultipleTimes()
        {
            List<GameObject> containers = new List<GameObject>();

            for (int i = 0; i < 6; i++)
            {
                avatarModel.wearables = new List<string>() { SUNGLASSES_ID };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
                containers.Add(GetWearableControlled(SUNGLASSES_ID)?.myAssetContainer);

                avatarModel.wearables = new List<string>() { };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            }

            Assert.IsTrue(containers.All(x => x == null));
        }

        [UnityTest]
        public IEnumerator BeRetrievedWithoutPoolableObject()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesAssetContainer = GetWearableControlled(SUNGLASSES_ID)?.myAssetContainer;
            var bandanaAssetContainer = GetWearableControlled(BLUE_BANDANA_ID)?.myAssetContainer;
            var sunglassesPoolableObject = sunglassesAssetContainer.GetComponentInChildren<PoolableObject>();
            var bandanaPoolableObject = bandanaAssetContainer.GetComponentInChildren<PoolableObject>();
            Assert.IsNull(sunglassesPoolableObject);
            Assert.IsNull(bandanaPoolableObject);
        }

        [UnityTest]
        public IEnumerator HideBodyShapeProperly()
        {
            catalog.Get(SUNGLASSES_ID).hides = new [] { WearableLiterals.Misc.HEAD };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var bodyShapeAssetContainer = GetBodyShapeController()?.myAssetContainer;
            Assert.IsNotNull(bodyShapeAssetContainer);

            var renderers = bodyShapeAssetContainer.GetComponentsInChildren<Renderer>();
            Assert.IsTrue(renderers.All(x => !x.enabled));
        }

        private WearableController_Mock GetWearableControlled(string id)
        {
            var wearableControllers = AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer);
            if (!wearableControllers.ContainsKey(id))
                return null;

            return new WearableController_Mock(wearableControllers[id]);
        }

        private BodyShapeController_Mock GetBodyShapeController()
        {
            var bodyShapeController = AvatarRenderer_Mock.GetBodyShape(avatarShape.avatarRenderer);
            if (bodyShapeController == null) return null;

            return new BodyShapeController_Mock(bodyShapeController);
        }
    }
}