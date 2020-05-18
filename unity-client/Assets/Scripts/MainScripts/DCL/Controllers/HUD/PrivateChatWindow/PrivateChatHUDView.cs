using DCL.Helpers;
using UnityEngine;

public class PrivateChatHUDView : ChatHUDView
{
    string ENTRY_PATH_SENT = "ChatEntrySent";
    string ENTRY_PATH_RECEIVED = "ChatEntryReceived";

    public override void AddEntry(ChatEntry.Model chatEntryModel)
    {
        var chatEntryGO = Instantiate(Resources.Load(chatEntryModel.subType == ChatEntry.Model.SubType.PRIVATE_TO ? ENTRY_PATH_SENT : ENTRY_PATH_RECEIVED) as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        chatEntry.SetFadeout(false);
        chatEntry.Populate(chatEntryModel);

        entries.Add(chatEntry);
        Utils.ForceUpdateLayout(transform as RectTransform);
    }
}
