using System;
using UnityEngine;

internal class ClipboardStandalone : IClipboardImplementation
{
    private Action<string, bool> onRead;

    void IClipboardImplementation.Initialize(Action onCopy, Action<string> onPaste, Action<string, bool> onRead)
    {
        // NOTE: there is no need for copy and paste input callback, Unity can handle it on standalone.
        this.onRead = onRead;
    }

    void IClipboardImplementation.RequestWriteText(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    void IClipboardImplementation.RequestGetText()
    {
        onRead?.Invoke(GUIUtility.systemCopyBuffer, false);
    }
}