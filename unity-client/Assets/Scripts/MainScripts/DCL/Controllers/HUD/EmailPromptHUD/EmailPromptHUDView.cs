using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

internal class EmailPromptHUDView : MonoBehaviour
{
    public event Action<string> OnSendEmail;
    public event Action<bool> OnDismiss;

    public TMP_InputField inputField;
    public Button_OnPointerDown closeButton;
    public Button_OnPointerDown sendButton;
    public Toggle dontAskAgain;
    public ShowHideAnimator showHideAnimator;

    void Awake()
    {
        sendButton.interactable = false;

        sendButton.onClick.AddListener(() => OnSendEmail?.Invoke(inputField.text));
        closeButton.onClick.AddListener(() => OnDismiss?.Invoke(dontAskAgain.isOn));

        inputField.onValueChanged.AddListener(value =>
        {
            sendButton.interactable = !string.IsNullOrEmpty(value);
        });

        showHideAnimator.OnWillFinishStart += OnWillFinishStart;
    }

    void OnWillFinishStart(ShowHideAnimator animator)
    {
        inputField.Select();
        inputField.ActivateInputField();
    }

    void OnDestroy()
    {
        showHideAnimator.OnWillFinishStart -= OnWillFinishStart;
    }
}
