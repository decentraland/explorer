using UnityEngine;
using UnityEngine.EventSystems;

public class VoiceChatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] InputAction_Hold voiceChatAction;
    [SerializeField] Animator buttonAnimator;
    [SerializeField] private VoiceChatDisabledTooltip tooltip;

    private static readonly int talkingAnimation = Animator.StringToHash("Talking");
    private static readonly int disabledAnimation = Animator.StringToHash("Disabled");

    private bool isRecording = false;
    private bool isEnabledByScene = true;
    private bool isFeatureLocked = true;

    private void Awake()
    {
        KernelConfig.i.OnChange += OnKernelConfigChanged;
        OnKernelConfigChanged(KernelConfig.i.Get(), null);
    }

    private void OnDestroy()
    {
        KernelConfig.i.OnChange -= OnKernelConfigChanged;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        voiceChatAction.RaiseOnStarted();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        voiceChatAction.RaiseOnFinished();
    }

    public void SetOnRecording(bool recording)
    {
        isRecording = recording;

        if (!gameObject.activeInHierarchy)
            return;

        buttonAnimator.SetBool(talkingAnimation, recording);
    }

    public void SetEnabledByScene(bool enabledByScene)
    {
        SetLockedByScene(!enabledByScene);
        isEnabledByScene = enabledByScene;
        buttonAnimator.SetBool(disabledAnimation, !isEnabledByScene);
    }

    private void OnVoiceChatInput(DCLAction_Hold action)
    {
        if (!isEnabledByScene || isFeatureLocked)
        {
            ShowDisabledTooltip();
        }
    }

    private void ShowDisabledTooltip()
    {
        tooltip.ShowTooltip();
    }

    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
    {
        isFeatureLocked = !current.comms.voiceChatEnabled;

        if (isFeatureLocked)
        {
            tooltip.SetLockedByFeatureMode();
            SubscribeToVoiceChatInput();
        }
        else
        {
            tooltip.SetLockedBySceneMode();
            SetLockedByScene(!isEnabledByScene);
        }
    }

    private void SetLockedByScene(bool locked)
    {
        if (isFeatureLocked)
        {
            return;
        }

        if (locked)
        {
            if (isRecording)
            {
                ShowDisabledTooltip();
            }

            SubscribeToVoiceChatInput();
        }
        else
        {
            UnsubscribeFromVoiceChatInput();
            tooltip.HideTooltip();
        }
    }

    private void SubscribeToVoiceChatInput()
    {
        voiceChatAction.OnStarted -= OnVoiceChatInput;
        voiceChatAction.OnStarted += OnVoiceChatInput;
    }

    private void UnsubscribeFromVoiceChatInput()
    {
        voiceChatAction.OnStarted -= OnVoiceChatInput;
    }
}