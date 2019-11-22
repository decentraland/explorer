using System;
using System.Collections;
using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;
    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    [SerializeField] CameraStateSO cameraState;
    [SerializeField] ThirdPersonCameraConfigSO thirdPersonConfig;
    [SerializeField] FirstPersonCameraConfigSO firstPersonConfig;

    private void Awake()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
        OnCameraStateOnChange(cameraState, cameraState);
        cameraState.OnChange += OnCameraStateOnChange;
    }

    private void OnCameraStateOnChange(CameraController.CameraState current, CameraController.CameraState previous)
    {
        switch (current)
        {
            case CameraController.CameraState.ThirdPerson:
                StopAllCoroutines();
                avatarRenderer.SetVisibility(true);
                break;
            case CameraController.CameraState.FirstPerson:
            default:
                StopAllCoroutines();
                StartCoroutine(SetVisibility(firstPersonConfig.Get().transitionTime, false));
                break;
        }
    }

    private IEnumerator SetVisibility(float delay, bool visibility)
    {
        yield return new WaitForSeconds(delay);
        avatarRenderer.SetVisibility(visibility);
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        OverrideModel(profile.avatar, null);
    }

    public void OverrideModel(AvatarModel model, Action onReady)
    {
        avatarRenderer.ApplyModel(model, onReady, null);
    }
}
