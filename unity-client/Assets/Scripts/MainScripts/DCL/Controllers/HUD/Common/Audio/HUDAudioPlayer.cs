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
        randomize,
        valueChange,
        fadeIn,
        fadeOut,
        notification,
        chatEntry,
        cameraToThirdPerson,
        cameraToFirstPerson
    }

    public static HUDAudioPlayer i { get; private set; }

    [HideInInspector]
    public AudioContainer ac;

    AudioEvent eventHover, eventClick, eventRelease, eventEnable, eventDisable, eventListItemAppear, eventDialogAppear, eventDialogClose, eventConfirm, eventCancel,
        eventValueChange, eventFadeIn, eventFadeOut;

    bool listItemAppearHasPlayed = false;
    float listItemAppearPitch = 1f;

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
        eventValueChange = ac.GetEvent("ValueChange");
        eventFadeIn = ac.GetEvent("FadeIn");
        eventFadeOut = ac.GetEvent("FadeOut");
    }

    private void Update()
    {
        listItemAppearHasPlayed = false;
    }

    public void Play(Sound sound, float pitch = 1f)
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
                eventEnable.Play();
                break;
            case Sound.valueChange:
                eventValueChange.SetPitch(pitch);
                eventValueChange.Play(true);
                break;
            case Sound.fadeIn:
                eventFadeIn.SetPitch(1f);
                eventFadeIn.Play();
                break;
            case Sound.fadeOut:
                eventFadeOut.Play();
                eventFadeOut.source.timeSamples = eventFadeOut.source.clip.samples - 1;
                eventFadeOut.source.pitch = -1f;
                break;
            case Sound.notification:
                eventFadeIn.SetPitch(3f);
                eventFadeIn.Play(true);
                break;
            case Sound.chatEntry:
                eventHover.SetPitch(3f);
                eventHover.Play(true);
                break;
            case Sound.cameraToFirstPerson:
                eventFadeOut.Play();
                eventFadeOut.source.timeSamples = eventFadeOut.source.clip.samples - 1;
                eventFadeOut.source.pitch = -0.85f;
                break;
            case Sound.cameraToThirdPerson:
                eventFadeIn.SetPitch(0.85f);
                eventFadeIn.Play(true);
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