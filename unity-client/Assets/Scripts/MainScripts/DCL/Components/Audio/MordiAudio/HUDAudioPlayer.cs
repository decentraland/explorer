using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDAudioPlayer : MonoBehaviour
{
    public enum Sound
    {
        none,
        buttonHover,
        buttonClick,
        buttonRelease,
        enable,
        disable,
        listItemAppear,
        dialogAppear,
        dialogClose,
        confirm,
        cancel,
        randomize
    }

    public static HUDAudioPlayer i { get; private set; }

    [HideInInspector]
    public AudioContainer ac;

    AudioEvent eventHover, eventClick, eventRelease, eventEnable, eventDisable, eventListItemAppear, eventDialogAppear, eventDialogClose, eventConfirm, eventCancel;

    bool listItemAppearHasPlayed = false;
    float listItemAppearPitch = 1f;

    float randomizeSoundTimer = 0f;
    int randomizeSoundCount = -1;

    private void Awake()
    {
        i = this;

        ac = GetComponent<AudioContainer>();
        eventHover = ac.GetEvent("ButtonHover");
        eventClick = ac.GetEvent("ButtonClick");
        eventRelease = ac.GetEvent("ButtonRelease");
        eventEnable = ac.GetEvent("Enable");
        eventDisable = ac.GetEvent("Disable");
        eventListItemAppear = ac.GetEvent("ListItemAppear");
        eventDialogAppear = ac.GetEvent("DialogAppear");
        eventDialogClose = ac.GetEvent("DialogClose");
        eventConfirm = ac.GetEvent("Confirm");
        eventCancel = ac.GetEvent("Cancel");
    }

    private void Update()
    {
        listItemAppearHasPlayed = false;

        // Handle randomize-sound
        if (randomizeSoundCount != -1)
        {
            randomizeSoundTimer -= Time.deltaTime;
            if (randomizeSoundTimer <= 0f)
            {
                randomizeSoundTimer = 0.005f + randomizeSoundCount / 70f;
                eventListItemAppear.SetPitch(2f + randomizeSoundCount / 5f);
                eventListItemAppear.Play(true);
                randomizeSoundCount += 1;

                if (randomizeSoundCount >= 10)
                {
                    eventListItemAppear.SetPitch(1f);
                    randomizeSoundTimer = 0f;
                    randomizeSoundCount = -1;
                }
            }
        }
    }

    public void Play(Sound sound)
    {
        switch (sound)
        {
            case Sound.buttonHover:
                eventHover.Play(true);
                break;
            case Sound.buttonClick:
                eventClick.Play();
                break;
            case Sound.buttonRelease:
                eventRelease.Play();
                break;
            case Sound.enable:
                eventEnable.Play();
                break;
            case Sound.disable:
                eventDisable.Play();
                break;
            case Sound.listItemAppear:
                if (!listItemAppearHasPlayed)
                {
                    eventListItemAppear.SetPitch(listItemAppearPitch);
                    eventListItemAppear.Play(true);
                    listItemAppearPitch += 0.15f;
                    listItemAppearHasPlayed = true;
                }
                break;
            case Sound.dialogAppear:
                eventDialogAppear.Play();
                break;
            case Sound.dialogClose:
                eventDialogClose.Play();
                break;
            case Sound.confirm:
                eventConfirm.Play();
                break;
            case Sound.cancel:
                eventCancel.Play();
                break;
            case Sound.randomize:
                randomizeSoundCount = 0;
                break;
            default:
                break;
        }
    }

    public void ResetListItemAppearPitch()
    {
        listItemAppearPitch = 1f;
    }
}