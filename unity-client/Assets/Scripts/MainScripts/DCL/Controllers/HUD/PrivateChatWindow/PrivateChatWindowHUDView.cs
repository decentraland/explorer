
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChatWindowHUDView : MonoBehaviour
{
    const string VIEW_PATH = "PrivateChatWindow";

    public Button backButton;
    public Button closeButton;
    public ChatHUDView chatHudView;
    public PrivateChatWindowHUDController controller;
    public TMP_Text windowTitleText;
    public Image profilePictureImage;

    public event System.Action OnPressBack;
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
        this.backButton.onClick.AddListener(() => { OnPressBack?.Invoke(); });
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

    public void ConfigureProfilePicture(Sprite sprite)
    {
        profilePictureImage.sprite = sprite;
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
