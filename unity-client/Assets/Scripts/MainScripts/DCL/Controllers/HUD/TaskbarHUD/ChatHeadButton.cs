using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHeadButton : TaskbarButton, IPointerEnterHandler, IPointerExitHandler
{
    internal GameObject labelContainer;
    internal TMPro.TextMeshProUGUI label;
    internal Button closeButton;

    internal Sprite portrait;
    internal ulong lastTimestamp;

    public event System.Action<ChatHeadButton> OnClose;

    public UserProfile profile;
    public void Initialize(UserProfile profile)
    {
        base.Initialize();
        this.profile = profile;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnCloseButtonPressed);

        if (profile.faceSnapshot != null)
            portrait = profile.faceSnapshot;
        else
            profile.OnFaceSnapshotReadyEvent += Profile_OnFaceSnapshotReadyEvent;
    }

    private void Profile_OnFaceSnapshotReadyEvent(Sprite portrait)
    {
        profile.OnFaceSnapshotReadyEvent -= Profile_OnFaceSnapshotReadyEvent;
        this.portrait = portrait;
    }

    private void OnCloseButtonPressed()
    {
        OnClose?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        labelContainer.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        labelContainer.SetActive(false);
    }
}
