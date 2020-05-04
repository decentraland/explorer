using UnityEngine;
using UnityEngine.UI;

public class FriendsHUDView : MonoBehaviour
{
    static int ANIM_PROPERTY_SELECTED = Animator.StringToHash("Selected");

    const string VIEW_PATH = "FriendsHUD";

    public Button closeButton;
    public Button friendsButton;
    public Button friendRequestsButton;
    public FriendsListView friendsList;
    public FriendRequestsListView friendRequestsList;

    public static FriendsHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FriendsHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        friendsList.Initialize();
        friendRequestsList.Initialize();

        closeButton.onClick.AddListener(Toggle);
        friendsButton.onClick.AddListener(() =>
        {
            friendsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
            friendRequestsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
            friendsList.gameObject.SetActive(true);
            friendRequestsList.gameObject.SetActive(false);
        });
        friendRequestsButton.onClick.AddListener(() =>
        {
            friendsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, false);
            friendRequestsButton.animator.SetBool(ANIM_PROPERTY_SELECTED, true);
            friendsList.gameObject.SetActive(false);
            friendRequestsList.gameObject.SetActive(true);
        });
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
