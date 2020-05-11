using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PrivateChatWindowHUDView : MonoBehaviour
{
    const string VIEW_PATH = "PrivateChatWindow";

    public Button closeButton;
    public ChatHUDView chatHudView;
    public PrivateChatWindowHUDController controller;
    public TMP_Text windowTitleText;

    public static PrivateChatWindowHUDView Create(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PrivateChatWindowHUDView>();
        view.Initialize(onPrivateMessages, onWorldMessages);
        return view;
    }

    private void Initialize(UnityAction onPrivateMessages, UnityAction onWorldMessages)
    {
        this.closeButton.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            chatHudView.ForceUpdateLayout();
        }
    }
}
