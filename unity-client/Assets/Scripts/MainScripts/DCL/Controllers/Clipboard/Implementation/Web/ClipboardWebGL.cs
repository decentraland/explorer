using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

internal class ClipboardWebGL : IClipboardImplementation
{
    private static ClipboardWebGL i;

    private Action onCopy;
    private Action<string, bool> onRead;

    private ClipboardWebGLPasteHandler pasteHandler;

    private delegate void ReadTextCallback(IntPtr ptrText, int intError);
    private delegate void OnPasteInputCallback(IntPtr ptrText);
    private delegate void OnCopyInputCallback();

    [DllImport("__Internal")]
    private static extern void initialize(Action<IntPtr, int> readTextCallback, Action<IntPtr> pasteCallback,
        Action copyCallback);

    [DllImport("__Internal")]
    private static extern void writeText(string text);

    [DllImport("__Internal")]
    private static extern void readText();

    [MonoPInvokeCallback(typeof(ReadTextCallback))]
    private static void OnReceiveReadText(IntPtr ptrText, int intError)
    {
        string value = Marshal.PtrToStringAuto(ptrText);
        bool error = intError == 0;
        i?.onRead?.Invoke(value, error);
    }

    [MonoPInvokeCallback(typeof(OnPasteInputCallback))]
    private static void OnReceivePasteInput(IntPtr ptrText)
    {
        string value = Marshal.PtrToStringAuto(ptrText);
        if (i != null && i.pasteHandler != null)
        {
            i.pasteHandler.pasteText = value;
        }
    }

    [MonoPInvokeCallback(typeof(OnCopyInputCallback))]
    private static void OnReceiveCopyInput()
    {
        i?.onCopy?.Invoke();
    }

    public ClipboardWebGL()
    {
        i = this;
        pasteHandler = new GameObject("_WebGLPasteHandler").AddComponent<ClipboardWebGLPasteHandler>();
    }

    void IClipboardImplementation.Initialize(Action onCopy, Action<string> onPaste, Action<string, bool> onRead)
    {
        this.onCopy = onCopy;
        this.onRead = onRead;
        pasteHandler.OnPasteCallback += onPaste;
        initialize(OnReceiveReadText, OnReceivePasteInput, OnReceiveCopyInput);
    }

    void IClipboardImplementation.RequestWriteText(string text)
    {
        writeText(text);
    }

    void IClipboardImplementation.RequestGetText()
    {
        readText();
    }
}