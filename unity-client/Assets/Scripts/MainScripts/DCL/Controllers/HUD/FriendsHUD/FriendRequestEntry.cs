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

    public event System.Action<FriendRequestEntry> OnAccepted;
    public event System.Action<FriendRequestEntry> OnRejected;
    public event System.Action<FriendRequestEntry> OnCancelled;

    public FriendEntry.Model model;
    public void Populate(FriendEntry.Model model, bool isReceived)
    {
        this.model = model;
        playerNameText.text = model.userName;
        playerImage.sprite = model.avatarImage;

        if (isReceived)
        {
            PopulateReceived();
        }
        else
        {
            PopulateSent();
        }
    }

    void PopulateReceived()
    {
        cancelButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(AcceptRequest);

        rejectButton.onClick.RemoveAllListeners();
        rejectButton.onClick.AddListener(RejectRequest);
    }

    void PopulateSent()
    {
        cancelButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(CancelRequest);
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
