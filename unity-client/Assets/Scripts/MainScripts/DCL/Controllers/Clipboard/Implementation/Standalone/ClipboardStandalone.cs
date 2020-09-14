using System;
using UnityEngine;

internal class ClipboardStandalone : IClipboardImplementation
{
    private Action<string, bool> onRead;

    void IClipboardImplementation.Initialize(Action<string, bool> onRead)
    {
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