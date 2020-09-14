using System;
using UnityEngine;

/// <summary>
/// We use this MonoBehaviour to move the Paste callback to the main thread
/// </summary>
internal class ClipboardWebGLPasteHandler : MonoBehaviour
{
    public event Action<string> OnPasteCallback;

    public string pasteText
    {
        set
        {
            text = value;
            enabled = true;
        }
    }

    private string text;

    private void Awake()
    {
        enabled = false;
    }

    private void Update()
    {
        enabled = false;
        OnPasteCallback?.Invoke(text);
    }
}