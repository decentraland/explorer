using System;
using System.Collections;
using DCL.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class UsersAroundListHUDListElementView : MonoBehaviour, IPoolLifecycleHandler
{
    const float USER_NOT_RECORDING_THROTTLING = 1;

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
    private Coroutine setUserRecordingRoutine = null;

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
        soundAnimator.SetTrigger(isMuted ? soundAnimationMute : soundAnimationUnmute);
    }

    public void SetRecording(bool isRecording)
    {
        if (setUserRecordingRoutine != null)
        {
            StopCoroutine(setUserRecordingRoutine);
        }
        setUserRecordingRoutine = StartCoroutine(SetRecordingRoutine(isRecording));
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
        if (setUserRecordingRoutine != null)
        {
            StopCoroutine(setUserRecordingRoutine);
            setUserRecordingRoutine = null;
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

    IEnumerator SetRecordingRoutine(bool isRecording)
    {
        if (isRecording)
        {
            micAnimator.SetTrigger(micAnimationRecording);
            yield break;
        }
        yield return WaitForSecondsCache.Get(USER_NOT_RECORDING_THROTTLING);
        micAnimator.SetTrigger(micAnimationIdle);
    }
}
