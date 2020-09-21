using System;

internal interface IClipboardHandler
{
    void Initialize(Action<string, bool> onRead);
    void RequestWriteText(string text);
    void RequestGetText();
}