using AvatarShape_Tests;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests
{
    public class AvatarShape_Pool_Tests : TestsBase
    {
        [UnityTest]
        public IEnumerator Pool_Recycle()
        {
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #1", "TestAvatar.json");

            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);

            Vector3 testPosition = Vector3.one * 20;
            TestHelpers.SetEntityTransform(scene, avatar.entity, testPosition, Quaternion.identity, Vector3.one);

            yield return null;

            TestHelpers.RemoveSceneEntity(scene, avatar.entity);

            yield return null;

            AvatarShape avatar2 = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #2", "TestAvatar.json");

            testPosition = Vector3.one * 10;
            TestHelpers.SetEntityTransform(scene, avatar.entity, testPosition, Quaternion.identity, Vector3.one);

            Assert.AreSame(avatar, avatar2);
        }
    }
}