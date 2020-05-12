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

    public static PrivateChatWindowHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PrivateChatWindowHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        this.closeButton.onClick.AddListener(Toggle);
    }

    public void ConfigureTitle(string targetUserName)
    {
        windowTitleText.text = targetUserName;
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
