using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;

public class AvatarAudioHandlerRemote : MonoBehaviour
{
    const float WALK_INTERVAL_SEC = 0.4f, RUN_INTERVAL_SEC = 0.27f;
    float intervalTimer = 0f;

    bool isVisible = true;
    AudioEvent footstepJump, footstepLand, footstepWalk, footstepRun, clothesRustleShort;
    CameraController cameraController;

    private void Start()
    {
        DCLCharacterController dclCharacterController = transform.parent.GetComponent<DCLCharacterController>();
        if (dclCharacterController != null)
        {
            dclCharacterController.OnJump += OnJump;
            dclCharacterController.OnHitGround += OnLand;
            dclCharacterController.OnMoved += OnWalk;
        }

        AudioContainer ac = GetComponent<AudioContainer>();
        footstepJump = ac.GetEvent("FootstepJump");
        footstepLand = ac.GetEvent("FootstepLand");
        footstepWalk = ac.GetEvent("FootstepWalk");
        footstepRun = ac.GetEvent("FootstepRun");
        clothesRustleShort = ac.GetEvent("ClothesRustleShort");

        AvatarMovementController avatarMovementController = GetComponentInParent<AvatarMovementController>();
        if (avatarMovementController != null)
        {
            avatarMovementController.OnMove += OnWalk;
        }

        AvatarVisibility avatarVisibility = GetComponentInParent<AvatarVisibility>();
        if (avatarVisibility != null)
        {
            avatarVisibility.OnSetVisibility += OnSetVisibility;
        }

        Debug.Log(avatarMovementController);
        Debug.Log(avatarVisibility);
    }

    void OnSetVisibility(bool visible)
    {
        Debug.Log("OnSetVisibility = " + visible);
        isVisible = visible;
    }

    void OnJump()
    {
        if (footstepJump != null)
            footstepJump.Play(true);
    }

    void OnLand()
    {
        if (footstepLand != null)
            footstepLand.Play(true);
    }

    // Faking footsteps when avatar is out of sight, since animations won't play
    void OnWalk(float distance)
    {
        Debug.Log("OnWalk - Distance = " + distance);

        if (isVisible)
            return;

        if (intervalTimer < 0f)
        {
            if (distance > 7.3f)
            {
                if (footstepRun != null)
                    footstepRun.Play(true);
                if (clothesRustleShort != null)
                    clothesRustleShort.Play(true);
                intervalTimer = RUN_INTERVAL_SEC;
            }
            else
            {
                if (footstepWalk != null)
                    footstepWalk.Play(true);
                if (clothesRustleShort != null)
                    clothesRustleShort.PlayScheduled(Random.Range(0.05f, 0.1f));
                intervalTimer = WALK_INTERVAL_SEC;
            }
        }

        intervalTimer -= Time.deltaTime;
    }
}
