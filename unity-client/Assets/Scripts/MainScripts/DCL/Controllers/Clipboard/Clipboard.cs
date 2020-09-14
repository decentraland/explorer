using System;
using System.Collections.Generic;

public static class Clipboard
{
    static readonly Queue<ClipboardReadPromise> promises = new Queue<ClipboardReadPromise>();
    private static readonly IClipboardImplementation impl = null;
    
    public static event Action<string> OnPasteInput;
    public static event Action OnCopyInput;

    public static void WriteText(string text)
    {
        impl?.RequestWriteText(text);
    }

    [Obsolete("Firefox not supported")]
    public static ClipboardReadPromise ReadText()
    {
        ClipboardReadPromise promise = new ClipboardReadPromise();
        promises.Enqueue(promise);
        impl?.RequestGetText();
        return promise;
    }

    static Clipboard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        impl = new ClipboardWebGL();
#else
        impl = new ClipboardStandalone();
#endif
        impl.Initialize(OnCopy, OnPaste, OnReadText);
    }

    private static void OnReadText(string text, bool error)
    {
        while (promises.Count > 0)
        {
            var promise = promises.Dequeue();
            promise.Resolve(text, error);
        }
    }

    private static void OnPaste(string text)
    {
        OnPasteInput?.Invoke(text);
    }

    private static void OnCopy()
    {
        OnCopyInput?.Invoke();
    }
}