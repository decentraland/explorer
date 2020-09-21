using System;
using System.Collections.Generic;
using DCL.Helpers;

public class Clipboard
{
    private readonly Queue<Promise<string>> promises = new Queue<Promise<string>>();
    private readonly IClipboardHandler impl = null;

    public void WriteText(string text)
    {
        impl?.RequestWriteText(text);
    }

    [Obsolete("Firefox not supported")]
    public Promise<string> ReadText()
    {
        Promise<string> promise = new Promise<string>();
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

    public Clipboard()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        impl = new ClipboardWebGL();
#else
        impl = new ClipboardStandalone();
#endif
        impl.Initialize(OnReadText);
    }

    private void OnReadText(string text, bool error)
    {
        while (promises.Count > 0)
        {
            var promise = promises.Dequeue();
            if (error)
            {
                promise.Reject(text);
            }
            else
            {
                promise.Resolve(text);
            }
        }
    }
}