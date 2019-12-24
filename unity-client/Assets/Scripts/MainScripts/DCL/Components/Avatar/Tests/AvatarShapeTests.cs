using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Tests
{
    public class AvatarShapeTests : TestsBase
    {

        void AssertMaterialsAreCorrect(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];

                for (int i1 = 0; i1 < renderer.sharedMaterials.Length; i1++)
                {
                    Material material = renderer.sharedMaterials[i1];
                    Assert.IsTrue(!material.shader.name.Contains("Lit"), $"Material must not be LWRP Lit. found {material.shader.name} instead!");
                }
            }
        }

        [UnityTest]
        public IEnumerator DestroyWhileLoading()
        {
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Abortit", "TestAvatar.json");

            yield return new WaitForSeconds(3.0f);

            GameObject goEntity = avatar.entity.gameObject;

            TestHelpers.RemoveSceneEntity(scene, avatar.entity);

            yield return null;

            bool destroyedOrPooled = goEntity == null || !goEntity.activeSelf;
            Assert.IsTrue(destroyedOrPooled);
        }

        [UnityTest]
        public IEnumerator MaterialsSetCorrectly()
        {
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Joan Darteis", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            AssertMaterialsAreCorrect(avatar.transform);
        }


        [UnityTest]
        public IEnumerator NameBackgroundHasCorrectSize()
        {
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Maiqel Yacson", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            avatar.transform.position = new Vector3(13, 0, 4);

            RectTransform rt = avatar.avatarName.uiContainer.GetComponent<RectTransform>();

            Assert.IsTrue((int)Mathf.Abs(rt.sizeDelta.x) == 190 && (int)Mathf.Abs(rt.sizeDelta.y) == 40, $"Avatar name dimensions are incorrect!. Current: {rt.sizeDelta}");
        }

        [UnityTest]
        public IEnumerator WhenTwoAvatarsLoadAtTheSameTimeTheyHaveProperMaterials()
        {
            //NOTE(Brian): Avatars must be equal to share their meshes.
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Naicholas Keig", "TestAvatar.json");
            AvatarShape avatar2 = AvatarTestHelpers.CreateAvatarShape(scene, "Naicholas Keig", "TestAvatar2.json");

            avatar.transform.position = new Vector3(-5, 0, 0);
            avatar2.transform.position = new Vector3(5, 0, 0);

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded && avatar2.everythingIsLoaded, 25);

            AssertMaterialsAreCorrect(avatar.transform);
            AssertMaterialsAreCorrect(avatar2.transform);
        }
    }
}
