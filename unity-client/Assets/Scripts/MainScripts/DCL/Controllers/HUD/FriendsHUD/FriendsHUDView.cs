using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendsHUDView : MonoBehaviour
{
    const string VIEW_PATH = "FriendsHUD";

    public Button closeButton;

    public Button onlineFriendsToggleButton;
    public GameObject onlineFriendsContainer;
    public Transform onlineFriendsToggleButtonIcon;

    public Button offlineFriendsToggleButton;
    public GameObject offlineFriendsContainer;
    public Transform offlineFriendsToggleButtonIcon;

    public static FriendsHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FriendsHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        closeButton.onClick.AddListener(Toggle);
        onlineFriendsToggleButton.onClick.AddListener(() =>
        {
            onlineFriendsContainer.SetActive(!onlineFriendsContainer.activeSelf);
            onlineFriendsToggleButtonIcon.localScale = new Vector3(onlineFriendsToggleButtonIcon.localScale.x, -onlineFriendsToggleButtonIcon.localScale.y, 1f);
        });
        offlineFriendsToggleButton.onClick.AddListener(() =>
        {
            offlineFriendsContainer.SetActive(!offlineFriendsContainer.activeSelf);
            offlineFriendsToggleButtonIcon.localScale = new Vector3(offlineFriendsToggleButtonIcon.localScale.x, -offlineFriendsToggleButtonIcon.localScale.y, 1f);
        });
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
