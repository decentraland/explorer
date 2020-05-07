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

    public event System.Action<FriendEntryBase> OnPassport;
    public event System.Action<FriendEntryBase> OnReport;
    public event System.Action<FriendEntryBase> OnBlock;
    public event System.Action<FriendEntryBase> OnDelete;


    public void Awake()
    {
        Debug.Log("awake!");
        passportButton.onClick.AddListener(OnPassportButtonPressed);
        reportButton.onClick.AddListener(OnReportUserButtonPressed);
        deleteButton.onClick.AddListener(OnDeleteUserButtonPressed);
        blockButton.onClick.AddListener(OnBlockUserButtonPressed);
    }

    internal FriendEntryBase targetEntry { get; private set; }

    internal void Toggle(FriendEntryBase entry)
    {
        transform.position = entry.menuPositionReference.position;

        Debug.Log(" Toggle called pre ... " + gameObject.activeSelf);

        gameObject.SetActive(targetEntry == entry ? !gameObject.activeSelf : true);

        Debug.Log(" Toggle called post ... " + gameObject.activeSelf);

        this.targetEntry = entry;

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
        Debug.Log("on passport pressed...");
        OnPassport?.Invoke(targetEntry);
        Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null || eventData.pointerPressRaycast.gameObject.layer != PhysicsLayers.friendsHUDPlayerMenu)
            Hide();
    }

}
