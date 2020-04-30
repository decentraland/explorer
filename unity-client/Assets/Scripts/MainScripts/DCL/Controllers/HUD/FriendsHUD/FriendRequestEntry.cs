using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestEntry : MonoBehaviour, IFriendEntry
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button acceptButton;
    [SerializeField] internal Button rejectButton;
    [SerializeField] internal Button cancelButton;

    public event System.Action<FriendRequestEntry> OnAccepted;
    public event System.Action<FriendRequestEntry> OnRejected;
    public event System.Action<FriendRequestEntry> OnCancelled;

    public string userId
    {
        get;
        private set;
    }
    public Transform menuPositionReference;

    public FriendEntry.Model model { get; private set; }

    public void Awake()
    {
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(AcceptRequest);

        rejectButton.onClick.RemoveAllListeners();
        rejectButton.onClick.AddListener(RejectRequest);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(CancelRequest);
    }

    public void Populate(string userId, FriendEntry.Model model, bool? isReceived = null)
    {
        this.userId = userId;
        this.model = model;
        playerNameText.text = model.userName;
        playerImage.sprite = model.avatarImage;

        if (isReceived.HasValue)
        {
            if (isReceived.Value)
            {
                PopulateReceived();
            }
            else
            {
                PopulateSent();
            }
        }
    }

    void PopulateReceived()
    {
        cancelButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
    }

    void PopulateSent()
    {
        cancelButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
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
