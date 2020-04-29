using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestEntry : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button acceptButton;
    [SerializeField] internal Button rejectButton;
    [SerializeField] internal Button cancelButton;

    public string friendId
    {
        get;
        private set;
    }

    public System.Action<FriendRequestEntry> OnAccepted;
    public System.Action<FriendRequestEntry> OnRejected;
    public System.Action<FriendRequestEntry> OnCancelled;

    public void Populate(string playerId, string playerName, Sprite playerAvatarImage, bool isReceived)
    {
        friendId = playerId;

        playerNameText.text = playerName;
        playerImage.sprite = playerAvatarImage;

        if (isReceived)
        {
            cancelButton.gameObject.SetActive(false);
            acceptButton.gameObject.SetActive(true);
            rejectButton.gameObject.SetActive(true);

            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(AcceptRequest);

            rejectButton.onClick.RemoveAllListeners();
            rejectButton.onClick.AddListener(RejectRequest);
        }
        else
        {
            cancelButton.gameObject.SetActive(true);
            acceptButton.gameObject.SetActive(false);
            rejectButton.gameObject.SetActive(false);

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelRequest);
        }
    }

    void AcceptRequest()
    {
        OnAccepted?.Invoke(this);
    }

    void RejectRequest()
    {
        OnRejected?.Invoke(this);
    }

    void CancelRequest()
    {
        OnCancelled?.Invoke(this);
    }
}

