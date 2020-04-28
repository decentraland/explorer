using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;

public class FriendRequestEntry : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button acceptButton;
    [SerializeField] internal Button rejectButton;
    [SerializeField] internal Button cancelButton;

    string playerId;
    public System.Action<string> OnRemoved;

    public void Populate(string playerName, Sprite playerAvatarImage, bool isReceived)
    {
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
        // TODO: Notify Kernel

        // TODO: Add to friends list

        OnRemoved?.Invoke(playerId);
        Destroy(gameObject);
    }

    void RejectRequest()
    {
        // TODO: Notify Kernel

        OnRemoved?.Invoke(playerId);
        Destroy(gameObject);
    }

    void CancelRequest()
    {
        // TODO: Notify Kernel

        OnRemoved?.Invoke(playerId);
        Destroy(gameObject);
    }
}

