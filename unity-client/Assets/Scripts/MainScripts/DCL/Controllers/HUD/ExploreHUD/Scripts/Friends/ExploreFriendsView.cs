using UnityEngine;
using UnityEngine.UI;

internal class ExploreFriendsView : MonoBehaviour
{
    [SerializeField] Image friendPortrait;

    UserProfile userProfile;

    public void SetUserProfile(UserProfile profile)
    {
        userProfile = profile;
        friendPortrait.sprite = profile.faceSnapshot;

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

    void OnDestroy()
    {
        if (userProfile)
        {
            userProfile.OnFaceSnapshotReadyEvent -= OnFaceSnapshotReadyEvent;
        }
    }
}
