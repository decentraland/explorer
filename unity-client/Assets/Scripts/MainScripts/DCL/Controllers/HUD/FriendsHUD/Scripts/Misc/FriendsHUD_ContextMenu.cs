using DCL.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsHUD_ContextMenu : MonoBehaviour, IPointerDownHandler
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";

    public Button passportButton;
    public Button blockButton;
    public TextMeshProUGUI blockButtonText;
    public Button reportButton;
    public Button deleteButton;

    public System.Action<FriendEntryBase> OnPassport;
    public System.Action<FriendEntryBase> OnReport;
    public System.Action<FriendEntryBase> OnBlock;
    public System.Action<FriendEntryBase> OnDelete;


    public void Awake()
    {
        passportButton.onClick.AddListener(OnPassportButtonPressed);
        reportButton.onClick.AddListener(OnReportUserButtonPressed);
        deleteButton.onClick.AddListener(OnDeleteUserButtonPressed);
        blockButton.onClick.AddListener(OnBlockUserButtonPressed);
    }

    internal FriendEntryBase targetEntry { get; private set; }

    internal void Toggle(FriendEntryBase entry)
    {
        this.targetEntry = entry;
        transform.position = entry.menuPositionReference.position;

        gameObject.SetActive(targetEntry == entry ? !gameObject.activeSelf : true);

        if (gameObject.activeSelf)
            blockButtonText.text = entry.model.blocked ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnBlockUserButtonPressed()
    {
        OnBlock?.Invoke(targetEntry);
        Hide();
    }

    private void OnDeleteUserButtonPressed()
    {
        OnDelete?.Invoke(targetEntry);
        Hide();
    }

    private void OnReportUserButtonPressed()
    {
        OnReport?.Invoke(targetEntry);
        Hide();
    }

    private void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(targetEntry);
        Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null || eventData.pointerPressRaycast.gameObject.layer != PhysicsLayers.friendsHUDPlayerMenu)
            Hide();
    }

}
