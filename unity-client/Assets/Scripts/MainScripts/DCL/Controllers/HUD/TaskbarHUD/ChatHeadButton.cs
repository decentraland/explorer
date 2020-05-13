using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatHeadButton : TaskbarButton, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] internal GameObject labelContainer;
    [SerializeField] internal TMPro.TextMeshProUGUI label;
    [SerializeField] internal Button closeButton;

    [SerializeField] internal Image portrait;
    internal ulong lastTimestamp;

    public event System.Action<ChatHeadButton> OnClose;

    internal UserProfile profile;
    public void Initialize(UserProfile profile)
    {
        base.Initialize();
        this.profile = profile;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnCloseButtonPressed);

        if (profile.faceSnapshot != null)
            portrait.sprite = profile.faceSnapshot;
        else
            profile.OnFaceSnapshotReadyEvent += Profile_OnFaceSnapshotReadyEvent;
    }

    private void Profile_OnFaceSnapshotReadyEvent(Sprite portraitSprite)
    {
        profile.OnFaceSnapshotReadyEvent -= Profile_OnFaceSnapshotReadyEvent;
        this.portrait.sprite = portraitSprite;
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
