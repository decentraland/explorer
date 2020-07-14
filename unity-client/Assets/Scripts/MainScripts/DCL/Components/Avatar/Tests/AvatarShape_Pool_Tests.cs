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

            yield return null;
            yield return null;
            yield return null;

            TestHelpers.RemoveSceneEntity(scene, avatar.entity);

            yield return null;

            AvatarShape avatar2 = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #2", "TestAvatar.json");

            Assert.AreSame(avatar, avatar2);
        }
    }
}