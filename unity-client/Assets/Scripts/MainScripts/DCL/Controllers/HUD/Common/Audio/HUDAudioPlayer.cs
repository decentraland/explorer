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
        sendChatEntry,
        receivePrivateChatEntry,
        receiveGlobalChatEntry,
        cameraToThirdPerson,
        cameraToFirstPerson,
        mapParcelHighlight
    }

    public static HUDAudioPlayer i { get; private set; }

    [HideInInspector]
    public AudioContainer ac;

    AudioEvent eventHover, eventClick, eventRelease, eventEnable, eventDisable, eventListItemAppear, eventDialogAppear, eventDialogClose, eventConfirm, eventCancel,
        eventValueChange, eventFadeIn, eventFadeOut, eventSendChatEntry, eventReceivePrivateChatEntry, eventReceiveGlobalChatEntry;

    bool listItemAppearHasPlayed = false;
    float listItemAppearPitch = 1f;
    
    public ulong timeAtStart;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(this);
            return;
        }

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
        eventSendChatEntry = ac.GetEvent("SendChatEntry");
        eventReceivePrivateChatEntry = ac.GetEvent("ReceivePrivateChatEntry");
        eventReceiveGlobalChatEntry = ac.GetEvent("ReceiveGlobalChatEntry");

        // Get UTC datetime at start (used to determine old/new chat messages)
        timeAtStart = (ulong)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
    }

    private void Update()
    {
        listItemAppearHasPlayed = false;
    }

    public void Play(Sound sound, float pitch = 1f)
    {
        // Stop sounds from playing while loading
        if (CommonScriptableObjects.rendererState != null)
        {
            if (!CommonScriptableObjects.rendererState.Get())
            {
                return;
            }
        }

        switch (sound)
        {
            case Sound.buttonHover:
                eventHover.SetPitch(1f);
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
                if (!eventDialogClose.source.isPlaying)
                    eventDialogAppear.Play(true);
                break;
            case Sound.dialogClose:
                // Prevent dialog appear and close playing at the same time
                if (!eventDialogAppear.source.isPlaying)
                    eventDialogClose.Play(true);
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
                eventFadeIn.Play(true);
                break;
            case Sound.fadeOut:
                eventFadeOut.SetPitch(1f);
                eventFadeOut.Play(true);
                break;
            case Sound.notification:
                eventFadeIn.SetPitch(3f);
                eventFadeIn.Play(true);
                break;
            case Sound.sendChatEntry:
                eventSendChatEntry.Play(true);
                break;
            case Sound.receivePrivateChatEntry:
                eventReceivePrivateChatEntry.Play(true);
                break;
            case Sound.receiveGlobalChatEntry:
                eventReceiveGlobalChatEntry.Play(true);
                break;
            case Sound.cameraToFirstPerson:
                eventFadeOut.SetPitch(0.85f);
                eventFadeOut.Play(true);
                break;
            case Sound.cameraToThirdPerson:
                eventFadeIn.SetPitch(0.85f);
                eventFadeIn.Play(true);
                break;
            case Sound.mapParcelHighlight:
                eventHover.SetPitch(3f);
                eventHover.Play(true);
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