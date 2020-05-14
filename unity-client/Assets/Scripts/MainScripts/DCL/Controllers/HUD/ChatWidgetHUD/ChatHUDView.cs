using DCL.Helpers;
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
    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    public ScrollRect scrollRect;
    public ChatHUDController controller;
    [NonSerialized] public List<ChatEntry> entries = new List<ChatEntry>();

    public static ChatHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatHUDView>();
        return view;
    }


    public void Initialize(ChatHUDController controller, UnityAction<string> onSendMessage)
    {
        this.controller = controller;
        inputField.onSubmit.AddListener(onSendMessage);
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
        inputField.Select();
        inputField.ActivateInputField();
    }


    public void AddEntry(ChatEntry.Model chatEntryModel)
    {
        var chatEntryGO = Instantiate(Resources.Load("Chat Entry") as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        chatEntry.Populate(chatEntryModel);

        entries.Add(chatEntry);

        SortEntries();

        Utils.ForceUpdateLayout(chatEntriesContainer, delayed: false);
    }

    public void SortEntries()
    {
        entries = entries.OrderBy(x => x.message.timestamp).ToList();

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
