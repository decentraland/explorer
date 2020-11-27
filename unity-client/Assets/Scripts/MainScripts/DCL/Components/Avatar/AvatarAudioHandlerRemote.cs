using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL;

public class AvatarAudioHandlerRemote : MonoBehaviour
{
    const float WALK_INTERVAL_SEC = 0.37f, RUN_INTERVAL_SEC = 0.25f;
    float nextFootstepTime = 0f;

    bool isVisible = true;
    AudioEvent footstepJump, footstepLand, footstepWalk, footstepRun, clothesRustleShort;
    GameObject rendererContainer;
    Renderer rend;
    AvatarAnimatorLegacy.BlackBoard blackBoard;
    bool isGroundedPrevious = true;

    public AvatarAnimatorLegacy avatarAnimatorLegacy;

    private void Start()
    {
        AudioContainer ac = GetComponent<AudioContainer>();
        footstepJump = ac.GetEvent("FootstepJump");
        footstepLand = ac.GetEvent("FootstepLand");
        footstepWalk = ac.GetEvent("FootstepWalk");
        footstepRun = ac.GetEvent("FootstepRun");
        clothesRustleShort = ac.GetEvent("ClothesRustleShort");

        if (avatarAnimatorLegacy != null)
        {
            blackBoard = avatarAnimatorLegacy.blackboard;
        }
    }

    public void Init(GameObject rendererContainer)
    {
        this.rendererContainer = rendererContainer;
    }

    private void Update()
    {
        if (blackBoard == null)
            return;

        // Jumped
        if (!blackBoard.isGrounded && isGroundedPrevious)
        {
            if (footstepJump != null)
                footstepJump.Play(true);
        }

        // Landed
        if (blackBoard.isGrounded && !isGroundedPrevious)
        {
            if (footstepLand != null)
                footstepLand.Play(true);
        }

        // Fake footsteps when avatar not visible
        if (rend != null)
        {
            if (!rend.isVisible && blackBoard.movementSpeed > 0f && blackBoard.isGrounded)
            {
                if (Time.time >= nextFootstepTime)
                {
                    if (blackBoard.movementSpeed > 0.045f)
                    {
                        if (footstepRun != null)
                            footstepRun.Play(true);
                        if (clothesRustleShort != null)
                            clothesRustleShort.Play(true);
                        nextFootstepTime = Time.time + RUN_INTERVAL_SEC;
                    }
                    else
                    {
                        if (footstepWalk != null)
                            footstepWalk.Play(true);
                        if (clothesRustleShort != null)
                            clothesRustleShort.PlayScheduled(Random.Range(0.05f, 0.1f));
                        nextFootstepTime = Time.time + WALK_INTERVAL_SEC;
                    }
                }
            }
        }
        else
        {
            if (rendererContainer != null)
            {
                //NOTE(Mordi): The renderer takes a while to get ready, so we need to check it continually until it can be fetched
                rend = rendererContainer.GetComponent<Renderer>();
            }
        }
        
        isGroundedPrevious = blackBoard.isGrounded;
    }
}
