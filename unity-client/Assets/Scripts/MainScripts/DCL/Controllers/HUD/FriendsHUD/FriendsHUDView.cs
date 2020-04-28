using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendsHUDView : MonoBehaviour
{
    const string VIEW_PATH = "FriendsHUD";

    public Button closeButton;
    public Button friendsButton;
    public Button friendRequestsButton;
    public GameObject friendsList;
    public GameObject friendRequestsList;

    public static FriendsHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FriendsHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        closeButton.onClick.AddListener(Toggle);
        friendsButton.onClick.AddListener(() =>
        {
            friendsList.SetActive(true);
            friendRequestsList.SetActive(false);
        });
        friendRequestsButton.onClick.AddListener(() =>
        {
            friendsList.SetActive(false);
            friendRequestsList.SetActive(true);
        });
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
