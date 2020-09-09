using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class ProfileHUDView : MonoBehaviour
{
    private const int ADDRESS_CHUNK_LENGTH = 6;

    [SerializeField] internal ShowHideAnimator showHideAnimator;

    [SerializeField] internal Image imageAvatarThumbnail;

    [SerializeField] internal TextMeshProUGUI textName;
    [SerializeField] internal TextMeshProUGUI textAddress;

    [SerializeField] internal Button buttonEditUnverifiedName;
    [SerializeField] internal Button buttonClaimName;
    [SerializeField] internal Button buttonCopyAddress;
    [SerializeField] internal Button buttonLogOut;
    [SerializeField] internal Button buttonToggleMenu;

    [SerializeField] internal GameObject loadingSpinner;

    private UserProfile profile = null;

    private void Awake()
    {
        buttonToggleMenu.onClick.AddListener(ToggleMenu);
    }

    public void SetProfile(UserProfile userProfile)
    {
        HandleProfileName(userProfile);
        HandleProfileAddress(userProfile);
        HandleProfileSnapshot(userProfile);
        profile = userProfile;
    }

    public void ToggleMenu()
    {
        if (showHideAnimator.isVisible)
        {
            showHideAnimator.Hide();
        }
        else
        {
            showHideAnimator.Show();
        }
    }

    private void HandleProfileSnapshot(UserProfile userProfile)
    {
        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetProfileImage;
        }

        if (userProfile.faceSnapshot != null)
        {
            SetProfileImage(userProfile.faceSnapshot);
        }
        else
        {
            loadingSpinner.SetActive(true);
            userProfile.OnFaceSnapshotReadyEvent += SetProfileImage;
        }
    }

    private void HandleProfileName(UserProfile userProfile)
    {
        textName.text = userProfile.userName;
    }

    private void HandleProfileAddress(UserProfile userProfile)
    {
        string address = userProfile.userId;
        string start = address.Substring(0, ADDRESS_CHUNK_LENGTH);
        string end = address.Substring(address.Length - ADDRESS_CHUNK_LENGTH);
        textAddress.text = $"{start}...{end}";
    }

    private void SetProfileImage(Sprite snapshot)
    {
        profile.OnFaceSnapshotReadyEvent -= SetProfileImage;
        imageAvatarThumbnail.sprite = snapshot;
        loadingSpinner.SetActive(false);
    }

    private void OnDestroy()
    {
        if (profile)
        {
            profile.OnFaceSnapshotReadyEvent -= SetProfileImage;
        }
    }
}
