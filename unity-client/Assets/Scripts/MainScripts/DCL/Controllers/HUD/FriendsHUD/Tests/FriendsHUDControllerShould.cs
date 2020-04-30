using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Tests
{
    class FriendsController_Mock : IFriendsController
    {
        public event Action<string, FriendsController.FriendshipAction> OnUpdateFriendship;
        public event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;

        public Dictionary<string, FriendsController.UserStatus> GetFriends()
        {
            return null;
        }

        public void RaiseUpdateFriendship(string id, FriendsController.FriendshipAction action)
        {
            OnUpdateFriendship?.Invoke(id, action);
        }

        public void RaiseUpdateUserStatus(string id, FriendsController.UserStatus userStatus)
        {
            OnUpdateUserStatus?.Invoke(id, userStatus);
        }
    }

    public class FriendsHUDControllerShould : TestsBase
    {
        FriendsHUDController controller;
        FriendsHUDView view;
        FriendsController_Mock friendsController;

        protected override bool justSceneSetUp => true;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            base.SetUp();

            controller = new FriendsHUDController();
            friendsController = new FriendsController_Mock();
            controller.Initialize(friendsController);
            this.view = controller.view;

            Assert.IsTrue(view != null, "Friends hud view is null?");
            Assert.IsTrue(controller != null, "Friends hud controller is null?");
            yield break;
        }

        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();
            UnityEngine.Object.Destroy(view);
        }

        [Test]
        public void ReactCorrectlyToKernelMessages()
        {
        }

        [Test]
        public void ReactCorrectlyToJumpInClick()
        {
            var id = "test-id-1";
            friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.APPROVED);

            bool jumpInCalled = false;

            WebInterface.OnMessageFromEngine += (name, payload) =>
            {
                if (name == "GoTo")
                {
                    jumpInCalled = true;
                }
            };

            var entry = controller.view.friendsList.GetEntry(id);
            entry.jumpInButton.onClick.Invoke();

            Assert.IsTrue(jumpInCalled);
        }

        [Test]
        public void ReactCorrectlyToWhisperClick()
        {
        }

        [Test]
        public void ReactCorrectlyToFriendApproved()
        {
            var id = "test-id-1";
            friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.APPROVED);

            WebInterface.OnMessageFromEngine += (name, payload) =>
            {
            };

            var entry = controller.view.friendsList.GetEntry(id);
        }

        [Test]
        public void ReactCorrectlyToFriendRejected()
        {
            var id = "test-id-1";
            friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.APPROVED);

            WebInterface.OnMessageFromEngine += (name, payload) =>
            {
            };

            var entry = controller.view.friendsList.GetEntry(id);
        }

        [Test]
        public void ReactCorrectlyToFriendCancelled()
        {
            var id = "test-id-1";
            friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.APPROVED);

            WebInterface.OnMessageFromEngine += (name, payload) =>
            {
            };

            var entry = controller.view.friendsList.GetEntry(id);
        }
    }
}
