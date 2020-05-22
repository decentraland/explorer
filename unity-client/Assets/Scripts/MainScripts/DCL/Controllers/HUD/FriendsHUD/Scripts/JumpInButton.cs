using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This button lets the player jump to the current location of a friend.
/// To initialize this control, use UpdateInfo().
/// </summary>
public class JumpInButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI playerLocationText;

    private IFriendsController currentFriendsController;
    private string currentUserId;
    private FriendsController.UserStatus currentUserStatus;

    private Vector2 currentCoords;
    private string currentRealmServerName;
    private string currentRealmLayerName;
    private PresenceStatus currentPresenceStatus;

    /// <summary>
    /// Prepares the JumpIn button for listening to a specific user.
    /// </summary>
    /// <param name="friendsController">Friends Controller to be listened</param>
    /// <param name="userId">User ID to listen to</param>
    public void Initialize(IFriendsController friendsController, string userId)
    {
        if (friendsController == null)
            return;

        currentFriendsController = friendsController;
        currentUserId = userId;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => JumpIn());

        currentFriendsController.OnUpdateUserStatus -= FriendsController_OnUpdateUserStatus;
        currentFriendsController.OnUpdateUserStatus += FriendsController_OnUpdateUserStatus;

        SearchUserStatus(currentUserId);
    }

    private void OnDestroy()
    {
        if (currentFriendsController == null)
            return;

        button.onClick.RemoveAllListeners();
        currentFriendsController.OnUpdateUserStatus -= FriendsController_OnUpdateUserStatus;
    }

    private void FriendsController_OnUpdateUserStatus(string userId, FriendsController.UserStatus userStatus)
    {
        if (userId != currentUserId)
            return;

        UpdateInfo(userStatus.position, userStatus.realm.serverName, userStatus.realm.layer, userStatus.presence);
    }

    private void SearchUserStatus(string userId)
    {
        if (FriendsController.i.GetFriends().TryGetValue(userId, out currentUserStatus))
        {
            UpdateInfo(
                currentUserStatus.position,
                currentUserStatus.realm != null ? currentUserStatus.realm.serverName : string.Empty,
                currentUserStatus.realm != null ? currentUserStatus.realm.layer : string.Empty,
                currentUserStatus.presence);
        }
    }

    private void UpdateInfo(Vector2 coords, string realmServerName, string realmLayerName, PresenceStatus status)
    {
        currentCoords = coords;
        currentRealmServerName = realmServerName;
        currentRealmLayerName = realmLayerName;
        currentPresenceStatus = status;

        ResfreshInfo();
    }

    private void ResfreshInfo()
    {
        if (currentPresenceStatus == PresenceStatus.ONLINE)
        {
            playerLocationText.text = $"{currentRealmServerName}-{currentRealmLayerName} {(int)currentCoords.x}, {(int)currentCoords.y}";
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
            playerLocationText.text = string.Empty;
        }
    }

    private void JumpIn()
    {
        WebInterface.JumpIn((int)currentCoords.x, (int)currentCoords.y, currentRealmServerName, currentRealmLayerName);
    }
}
