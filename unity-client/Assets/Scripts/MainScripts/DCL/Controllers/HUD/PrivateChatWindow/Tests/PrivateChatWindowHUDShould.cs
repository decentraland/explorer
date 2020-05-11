﻿using DCL;
using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

public class PrivateChatWindowHUDShould : TestsBase
{
    private PrivateChatWindowHUDController controller;
    private PrivateChatWindowHUDView view;
    private ChatController_Mock chatController;
    private MouseCatcher_Mock mouseCatcher;

    private UserProfileModel ownProfileModel;
    private UserProfileModel testProfileModel;

    protected override bool justSceneSetUp => true;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        UserProfileController.i.ClearProfilesCatalog();

        var ownProfile = UserProfile.GetOwnUserProfile();

        ownProfileModel = new UserProfileModel();
        ownProfileModel.userId = "my-user-id";
        ownProfileModel.name = "NO_USER";
        ownProfile.UpdateData(ownProfileModel, false);

        testProfileModel = new UserProfileModel();
        testProfileModel.userId = "my-user-id-2";
        testProfileModel.name = "TEST_USER";
        UserProfileController.i.AddUserProfileToCatalog(testProfileModel);

        //NOTE(Brian): This profile is added by the LoadProfile message in the normal flow.
        //             Adding this here because its used by the chat flow in ChatMessageToChatEntry.
        UserProfileController.i.AddUserProfileToCatalog(ownProfileModel);

        controller = new PrivateChatWindowHUDController();
        chatController = new ChatController_Mock();
        mouseCatcher = new MouseCatcher_Mock();
        controller.Initialize(chatController, mouseCatcher);
        this.view = controller.view;
        Assert.IsTrue(view != null, "World chat hud view is null?");
        Assert.IsTrue(controller != null, "World chat hud controller is null?");

        yield break;
    }

    [Test]
    public void HandleChatControllerProperly()
    {
        var chatMessage = new ChatMessage()
        {
            messageType = ChatMessage.Type.PUBLIC,
            body = "test message",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        var entry = controller.view.chatHudView.entries[0];

        var chatEntryModel = ChatHUDController.ChatMessageToChatEntry(chatMessage);

        Assert.AreEqual(entry.message, chatEntryModel);
    }

        [UnityTest]
    public IEnumerator SendChatMessageProperly()
    {
        bool messageWasSent = false;

        System.Action<string, string> messageCallback =
            (type, msg) =>
            {
                if (type == "SendChatMessage" && msg.Contains("test message"))
                {
                    messageWasSent = true;
                }
            };

        WebInterface.OnMessageFromEngine += messageCallback;
        controller.resetInputFieldOnSubmit = false;
        controller.SendChatMessage("test message");
        Assert.IsTrue(messageWasSent);
        Assert.AreEqual("", controller.view.chatHudView.inputField.text);
        WebInterface.OnMessageFromEngine -= messageCallback;
        yield break;
    }

    [Test]
    public void CloseWhenButtonPressed()
    {
        controller.SetVisibility(true);
        Assert.AreEqual(true, controller.view.gameObject.activeSelf);
        controller.view.closeButton.onClick.Invoke();
        Assert.AreEqual(false, controller.view.gameObject.activeSelf);
    }
}
