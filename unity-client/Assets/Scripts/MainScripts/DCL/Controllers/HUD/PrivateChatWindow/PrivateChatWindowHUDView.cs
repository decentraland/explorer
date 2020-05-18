using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChatWindowHUDView : MonoBehaviour
{
    const string VIEW_PATH = "PrivateChatWindow";

    public Button closeButton;
    public ChatHUDView chatHudView;
    public PrivateChatWindowHUDController controller;
    public TMP_Text windowTitleText;

    public event System.Action OnClose;


    void OnEnable()
    {
        DCL.Helpers.Utils.ForceUpdateLayout(transform as RectTransform);
    }

    public static PrivateChatWindowHUDView Create(PrivateChatWindowHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PrivateChatWindowHUDView>();
        view.Initialize(controller);
        return view;
    }

    private void Initialize(PrivateChatWindowHUDController controller)
    {
        this.controller = controller;
        this.closeButton.onClick.AddListener(OnCloseButtonPressed);
    }
    public void OnCloseButtonPressed()
    {
        controller.SetVisibility(false);
        OnClose?.Invoke();
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
        }
    }


}
