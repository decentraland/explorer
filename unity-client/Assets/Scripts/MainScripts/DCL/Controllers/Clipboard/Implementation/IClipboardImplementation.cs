using System;

internal interface IClipboardImplementation
{
    void Initialize(Action onCopy, Action<string> onPaste, Action<string, bool> onRead);
    void RequestWriteText(string text);
    void RequestGetText();
}