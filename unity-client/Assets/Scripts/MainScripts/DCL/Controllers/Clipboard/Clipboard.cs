#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_PLATFORM
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

#if WEB_PLATFORM
using System.Runtime.InteropServices;
using AOT;
#endif

public static class Clipboard
{
#if WEB_PLATFORM
    private delegate void ReadTextCallback(IntPtr ptrText, int intError);
    private delegate void OnPasteTextCallback(IntPtr ptrText);

    [DllImport("__Internal")]
    private static extern void initialize(Action<IntPtr,int> readTextCallback, Action<IntPtr> pasteCallback);

    [DllImport("__Internal")]
    private static extern void writeText(string text);

    [DllImport("__Internal")]
    private static extern void readText();

    [MonoPInvokeCallback(typeof(ReadTextCallback))]
    private static void OnReceiveReadText(IntPtr ptrText, int intError)
    {
        string value = Marshal.PtrToStringAuto(ptrText);
        bool error = intError == 0;
        OnReadText(value, error);
    }

    [MonoPInvokeCallback(typeof(OnPasteTextCallback))]
    private static void OnReceivePasteText(IntPtr ptrText)
    {
        string value = Marshal.PtrToStringAuto(ptrText);
        OnPaste(value);
    }
#else
    private static void writeText(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    private static void readText()
    {
        OnReadText(GUIUtility.systemCopyBuffer, false);
    }
#endif

    static readonly Queue<ClipboardReadPromise> promises = new Queue<ClipboardReadPromise>();

    public static event Action<string> OnPasteInput;

    public static void WriteText(string text)
    {
        writeText(text);
    }

    [Obsolete("Firefox not supported ")]
    public static ClipboardReadPromise ReadText()
    {
        ClipboardReadPromise promise = new ClipboardReadPromise();
        promises.Enqueue(promise);
        readText();
        return promise;
    }

    static Clipboard()
    {
#if WEB_PLATFORM
        initialize(OnReceiveReadText, OnReceivePasteText);
#endif
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
}

public class ClipboardReadPromise : CustomYieldInstruction
{
    private bool resolved = false;
    private bool failed = false;

    private Action<string> onSuccess;
    private Action<string> onError;

    public override bool keepWaiting => !resolved;
    public string value { private set; get; }
    public string error { private set; get; }

    internal void Resolve(string result, bool isError)
    {
        failed = isError;

        if (isError)
        {
            value = null;
            error = value;
            onError?.Invoke(result);
        }
        else
        {
            value = result;
            error = null;
            onSuccess?.Invoke(result);
        }

        resolved = true;
    }

    internal ClipboardReadPromise() { }

    public ClipboardReadPromise Then(Action<string> success)
    {
        if (!resolved)
        {
            onSuccess = success;
        }
        else if (!failed)
        {
            success?.Invoke(value);
        }

        return this;
    }

    public void Catch(Action<string> error)
    {
        if (!resolved)
        {
            onError = error;
        }
        else if (failed)
        {
            error?.Invoke(value);
        }
    }
}