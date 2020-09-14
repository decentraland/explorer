using System;
using System.Collections.Generic;

public static class Clipboard
{
    static readonly Queue<ClipboardReadPromise> promises = new Queue<ClipboardReadPromise>();
    private static readonly IClipboardImplementation impl = null;

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

#if UNITY_WEBGL && !UNITY_EDITOR
    public static void HookBrowserCopyPasteInput()
    {
        // NOTE: this does nothing but we'll use it to force the instantiation of this static class
        // cause you need to call something for a static class to be instantiated.
        // all the hooking is actually done inside ClipboardWebGL class.
    }
#endif

    static Clipboard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        impl = new ClipboardWebGL();
#else
        impl = new ClipboardStandalone();
#endif
        impl.Initialize(OnReadText);
    }

    private static void OnReadText(string text, bool error)
    {
        while (promises.Count > 0)
        {
            var promise = promises.Dequeue();
            promise.Resolve(text, error);
        }
    }
}