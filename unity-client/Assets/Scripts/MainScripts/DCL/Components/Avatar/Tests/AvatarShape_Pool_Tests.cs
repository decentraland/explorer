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
        public IEnumerator Avatar_Reposition_Test()
        {
            Debug.Log("Avatar #1...");
            AvatarTestHelpers.CreateTestCatalog();
            AvatarShape avatar = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #1", "TestAvatar.json");
            yield return new DCL.WaitUntil(() => avatar.everythingIsLoaded, 20);
            Vector3 testPosition = Vector3.one * 20;
            TestHelpers.SetEntityTransform(scene, avatar.entity, testPosition, Quaternion.identity, Vector3.one);
            yield return null;
            TestHelpers.RemoveSceneEntity(scene, avatar.entity);
            yield return null;
            Debug.Log("Avatar #2...");
            AvatarShape avatar2 = AvatarTestHelpers.CreateAvatarShape(scene, "Avatar #2", "TestAvatar.json");

            testPosition = Vector3.one * 10;
            TestHelpers.SetEntityTransform(scene, avatar.entity, testPosition, Quaternion.identity, Vector3.one);

            yield return new DCL.WaitUntil(() => avatar2.everythingIsLoaded, 20);
            yield return new WaitForSeconds(5.0f);
            Debug.Break();
            yield return null;
        }
    }
}