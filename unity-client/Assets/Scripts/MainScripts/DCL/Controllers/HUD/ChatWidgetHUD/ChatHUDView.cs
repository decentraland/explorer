using DCL.Helpers;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ChatHUDView : MonoBehaviour
{
    static string VIEW_PATH = "Chat Widget";
    string ENTRY_PATH = "Chat Entry";

    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    public ScrollRect scrollRect;
    public ChatHUDController controller;
    public GameObject messageHoverPanel;
    public TextMeshProUGUI messageHoverText;
    [NonSerialized] public List<ChatEntry> entries = new List<ChatEntry>();

    public event UnityAction<string> OnPressPrivateMessage;
    private UnityAction<string> OnSendMessageAction;

    public static ChatHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatHUDView>();
        return view;
    }

    public void Initialize(ChatHUDController controller, UnityAction<string> OnSendMessage)
    {
        this.controller = controller;
        OnSendMessageAction = OnSendMessage;
        inputField.onSubmit.AddListener(OnInputFieldSubmit);
    }

    private void OnInputFieldSubmit(string message)
    {
        // A TMP_InputField is automatically marked as 'wasCanceled' when the ESC key is pressed
        if (inputField.wasCanceled)
            message = "";

        OnSendMessageAction(message);
    }

    public void ResetInputField()
    {
        inputField.text = "";
        inputField.caretColor = Color.white;
    }

    void OnEnable()
    {
        Utils.ForceUpdateLayout(transform as RectTransform);
    }

    public void FocusInputField()
    {
        inputField.ActivateInputField();
        inputField.Select();
    }

    bool enableFadeoutMode = false;

    bool EntryIsVisible(ChatEntry entry)
    {
        int visibleCorners = (entry.transform as RectTransform).CountCornersVisibleFrom(scrollRect.viewport.transform as RectTransform);
        return visibleCorners > 0;
    }

    public void SetFadeoutMode(bool enabled)
    {
        enableFadeoutMode = enabled;

        for (int i = 0; i < entries.Count; i++)
        {
            ChatEntry entry = entries[i];

            if (enabled)
            {
                entry.SetFadeout(EntryIsVisible(entry));
            }
            else
            {
                entry.SetFadeout(false);
            }
        }
    }

    public virtual void AddEntry(ChatEntry.Model chatEntryModel)
    {
        var chatEntryGO = Instantiate(Resources.Load(ENTRY_PATH) as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        if (enableFadeoutMode && EntryIsVisible(chatEntry))
            chatEntry.SetFadeout(true);
        else
            chatEntry.SetFadeout(false);

        chatEntry.Populate(chatEntryModel);

        if (chatEntryModel.messageType == ChatMessage.Type.PRIVATE)
            chatEntry.OnPress += OnPressPrivateMessage;

        chatEntry.OnTriggerHover += OnMessageTriggerHover;
        chatEntry.OnCancelHover += OnMessageCancelHover;

        entries.Add(chatEntry);

        SortEntries();

        Utils.ForceUpdateLayout(transform as RectTransform, delayed: false);
    }

    protected virtual void OnMessageTriggerHover(ChatEntry chatEntry)
    {
        messageHoverText.text = chatEntry.messageLocalDateTime;
        messageHoverPanel.transform.position = chatEntry.hoverPanelPositionReference.position;
        messageHoverPanel.SetActive(true);
    }

    protected void OnMessageCancelHover()
    {
        messageHoverPanel.SetActive(false);
        messageHoverText.text = string.Empty;
    }

    public void SortEntries()
    {
        entries = entries.OrderBy(x => x.model.timestamp).ToList();

        int count = entries.Count;
        for (int i = 0; i < count; i++)
        {
            entries[i].transform.SetSiblingIndex(i);
        }
    }

    public void CleanAllEntries()
    {
        foreach (var entry in entries)
        {
            Destroy(entry.gameObject);
        }

        entries.Clear();
    }
}
