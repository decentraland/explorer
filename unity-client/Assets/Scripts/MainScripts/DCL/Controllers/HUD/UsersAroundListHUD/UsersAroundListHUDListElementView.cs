using System;
using DCL.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class UsersAroundListHUDListElementView : MonoBehaviour, IPoolLifecycleHandler
{
    public event Action<string, bool> OnMuteUser;

    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal Image avatarPreview;
    [SerializeField] internal Animator micAnimator;
    [SerializeField] internal Animator soundAnimator;
    [SerializeField] internal Button soundButton;

    private static readonly int micAnimationIdle = Animator.StringToHash("Idle");
    private static readonly int micAnimationRecording = Animator.StringToHash("Recording");
    private static readonly int soundAnimationMute = Animator.StringToHash("Mute");
    private static readonly int soundAnimationUnmute = Animator.StringToHash("Unmute");

    private UserProfile profile;
    private bool isMuted = false;

    private void Start()
    {
        soundButton.onClick.AddListener(OnSoundButtonPressed);
    }

    public void SetUserProfile(string userId)
    {
        profile = UserProfileController.userProfilesCatalog.Get(userId);
        userName.text = profile.userName;

        if (profile.faceSnapshot)
        {
            SetAvatarPreviewImage(profile.faceSnapshot);
        }
        else
        {
            profile.OnFaceSnapshotReadyEvent += SetAvatarPreviewImage;
        }
    }

    public void SetMuted(bool isMuted)
    {
        if (this.isMuted == isMuted)
        {
            return;
        }

        this.isMuted = isMuted;
        soundAnimator.SetTrigger(isMuted? soundAnimationMute : soundAnimationUnmute);
    }

    public void SetRecording(bool isRecording)
    {
        micAnimator.SetTrigger(isRecording? micAnimationRecording : micAnimationIdle);
    }

    public void OnPoolRelease()
    {
        avatarPreview.sprite = null;
        userName.text = string.Empty;
        isMuted = false;

        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetAvatarPreviewImage;
            profile = null;
        }
        gameObject.SetActive(false);
    }

    public void OnPoolGet()
    {
        micAnimator.SetTrigger(micAnimationIdle);
        soundAnimator.SetTrigger(soundAnimationUnmute);
        avatarPreview.sprite = null;
        userName.text = string.Empty;
        gameObject.SetActive(true);
    }

    void SetAvatarPreviewImage(Sprite sprite)
    {
        avatarPreview.sprite = sprite;
    }

    void OnSoundButtonPressed()
    {
        if (profile == null)
        {
            return;
        }
        OnMuteUser?.Invoke(profile.userId, !isMuted);
    }
}
