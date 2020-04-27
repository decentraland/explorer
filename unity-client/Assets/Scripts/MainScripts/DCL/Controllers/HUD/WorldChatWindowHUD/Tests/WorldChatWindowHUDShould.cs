using DCL;
using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

class ChatController_Mock : IChatController
{
    public event Action<ChatController.ChatMessage> OnAddMessage;
    List<ChatController.ChatMessage> entries = new List<ChatController.ChatMessage>();
    public List<ChatController.ChatMessage> GetEntries()
    {
        return entries;
    }

    public void RaiseAddMessage(ChatController.ChatMessage chatMessage)
    {
        entries.Add(chatMessage);
        OnAddMessage?.Invoke(chatMessage);
    }
}
class MouseCatcher_Mock : IMouseCatcher
{
    public event Action OnMouseUnlock;
    public event Action OnMouseLock;

    public void RaiseMouseUnlock() { OnMouseUnlock?.Invoke(); }
    public void RaiseMouseLock() { OnMouseLock?.Invoke(); }
}

public class WorldChatWindowHUDShould : TestsBase
{
    private WorldChatWindowHUDController controller;
    private WorldChatWindowHUDView view;
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

        controller = new WorldChatWindowHUDController();
        chatController = new ChatController_Mock();
        mouseCatcher = new MouseCatcher_Mock();
        controller.Initialize(chatController, mouseCatcher);
        this.view = controller.view;
        Assert.IsTrue(view != null, "World chat hud view is null?");
        Assert.IsTrue(controller != null, "World chat hud controller is null?");

        yield break;
    }

    [Test]
    public void TabsWorkCorrectly()
    {

        var messages = new ChatController.ChatMessage[]
        {
            new ChatController.ChatMessage()
            {
                messageType = ChatController.ChatMessageType.PUBLIC,
                body = "test message 1",
                sender = ownProfileModel.userId,
                recipient = testProfileModel.userId
            },
            new ChatController.ChatMessage()
            {
                messageType = ChatController.ChatMessageType.PUBLIC,
                body = "test message 2",
                sender = ownProfileModel.userId,
                recipient = testProfileModel.userId
            },
            new ChatController.ChatMessage()
            {
                messageType = ChatController.ChatMessageType.PRIVATE,
                body = "test message 3",
                sender = ownProfileModel.userId,
                recipient = testProfileModel.userId
            },
            new ChatController.ChatMessage()
            {
                messageType = ChatController.ChatMessageType.PRIVATE,
                body = "test message 4",
                sender = ownProfileModel.userId,
                recipient = testProfileModel.userId
            },
        };

        foreach (var msg in messages)
        {
            chatController.RaiseAddMessage(msg);
        }

        var expectedBodyMessages = new string[]
        {
            "<b>NO_USER:</b> test message 1",
            "<b>NO_USER:</b> test message 2",
            "<b>[To TEST_USER]:</b> test message 3",
            "<b>[To TEST_USER]:</b> test message 4"
        };

        Assert.AreEqual(4, controller.view.chatHudView.entries.Count);

        for (int i = 0; i < controller.view.chatHudView.entries.Count; i++)
        {
            ChatEntry entry = controller.view.chatHudView.entries[i];
            Assert.AreEqual(expectedBodyMessages[i], entry.body.text);
        }

        controller.view.pmFilterButton.onClick.Invoke();

        expectedBodyMessages = new string[]
        {
            "<b>[To TEST_USER]:</b> test message 3",
            "<b>[To TEST_USER]:</b> test message 4"
        };

        Assert.AreEqual(2, controller.view.chatHudView.entries.Count);

        for (int i = 0; i < controller.view.chatHudView.entries.Count; i++)
        {
            ChatEntry entry = controller.view.chatHudView.entries[i];
            Assert.AreEqual(expectedBodyMessages[i], entry.body.text);
        }

        controller.view.worldFilterButton.onClick.Invoke();
    }

    [Test]
    public void HandlePrivateMessagesProperly()
    {
        var sentPM = new ChatController.ChatMessage()
        {
            messageType = ChatController.ChatMessageType.PRIVATE,
            body = "test message",
            sender = ownProfileModel.userId,
            recipient = testProfileModel.userId
        };

        chatController.RaiseAddMessage(sentPM);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        ChatEntry entry = controller.view.chatHudView.entries[0];

        Assert.AreEqual("<b>[To TEST_USER]:</b>", entry.username.text);
        Assert.AreEqual("<b>[To TEST_USER]:</b> test message", entry.body.text);

        var receivedPM = new ChatController.ChatMessage()
        {
            messageType = ChatController.ChatMessageType.PRIVATE,
            body = "test message",
            sender = testProfileModel.userId,
            recipient = ownProfileModel.userId
        };

        chatController.RaiseAddMessage(receivedPM);

        ChatEntry entry2 = controller.view.chatHudView.entries[1];

        Assert.AreEqual("<b>[From TEST_USER]:</b>", entry2.username.text);
        Assert.AreEqual("<b>[From TEST_USER]:</b> test message", entry2.body.text);
    }


    [Test]
    public void HandleChatControllerProperly()
    {
        var chatMessage = new ChatController.ChatMessage()
        {
            messageType = ChatController.ChatMessageType.PUBLIC,
            body = "test message",
            sender = testProfileModel.userId
        };

        chatController.RaiseAddMessage(chatMessage);

        Assert.AreEqual(1, controller.view.chatHudView.entries.Count);

        var entry = controller.view.chatHudView.entries[0];

        var chatEntryModel = ChatHUDController.ChatMessageToChatEntry(chatMessage);

        Assert.AreEqual(entry.message, chatEntryModel);
    }

    [Test]
    public void HandleMouseCatcherProperly()
    {
        mouseCatcher.RaiseMouseLock();
        Assert.AreEqual(0, view.group.alpha);
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
