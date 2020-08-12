using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal class ExploreFriendsView : MonoBehaviour
{
    [SerializeField] Image friendPortrait;
    [SerializeField] ShowHideAnimator showHideAnimator;
    [SerializeField] TextMeshProUGUI friendName;
    [SerializeField] UIHoverCallback hoverCallback;

    UserProfile userProfile;

    public void SetUserProfile(UserProfile profile)
    {
        userProfile = profile;
        friendPortrait.sprite = profile.faceSnapshot;
        friendName.text = profile.userName;

        if (profile.faceSnapshot == null)
        {
            gameObject.SetActive(false);
            profile.OnFaceSnapshotReadyEvent += OnFaceSnapshotReadyEvent;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    void OnFaceSnapshotReadyEvent(Sprite sprite)
    {
        userProfile.OnFaceSnapshotReadyEvent -= OnFaceSnapshotReadyEvent;
        friendPortrait.sprite = userProfile.faceSnapshot;
        gameObject.SetActive(true);
    }

    void OnHeadHoverEnter()
    {
        if (userProfile)
        {
            if (!showHideAnimator.gameObject.activeSelf)
            {
                showHideAnimator.gameObject.SetActive(true);
            }
            showHideAnimator.Show();
        }
    }

    void OnHeadHoverExit()
    {
        showHideAnimator.Hide();
    }

    void Awake()
    {
        hoverCallback.OnPointerEnter += OnHeadHoverEnter;
        hoverCallback.OnPointerExit += OnHeadHoverExit;
        showHideAnimator.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (userProfile)
        {
            userProfile.OnFaceSnapshotReadyEvent -= OnFaceSnapshotReadyEvent;
        }

        if (hoverCallback)
        {
            hoverCallback.OnPointerEnter -= OnHeadHoverEnter;
            hoverCallback.OnPointerExit -= OnHeadHoverExit;
        }
    }
}
