using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAudioHandler : MonoBehaviour
{
    AudioEvent footstepJump, footstepLand;

    private void Start()
    {
        DCLCharacterController dclCharacterController = transform.parent.GetComponent<DCLCharacterController>();
        if (dclCharacterController != null)
        {
            dclCharacterController.OnJump += OnJump;
            dclCharacterController.OnHitGround += OnLand;
        }

        AudioContainer ac = GetComponent<AudioContainer>();
        footstepJump = ac.GetEvent("FootstepJump");
        footstepLand = ac.GetEvent("FootstepLand");
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
}
