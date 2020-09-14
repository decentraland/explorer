using System;

internal interface IClipboardImplementation
{
    void Initialize(Action<string, bool> onRead);
    void RequestWriteText(string text);
    void RequestGetText();
}