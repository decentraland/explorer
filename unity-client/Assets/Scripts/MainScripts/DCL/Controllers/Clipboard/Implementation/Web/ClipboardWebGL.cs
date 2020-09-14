using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

internal class ClipboardWebGL : IClipboardImplementation, IDisposable
{
    private static ClipboardWebGL i;

    private Action<string, bool> onRead;
    private bool copyInput = false;

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
        GUIUtility.systemCopyBuffer = value;
    }

    [MonoPInvokeCallback(typeof(OnCopyInputCallback))]
    private static void OnReceiveCopyInput()
    {
        if (i != null) i.copyInput = true;
    }

    public ClipboardWebGL()
    {
        i = this;
        Application.onBeforeRender += OnBeforeRender;
    }

    public void Dispose()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    void IClipboardImplementation.Initialize(Action<string, bool> onRead)
    {
        this.onRead = onRead;
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

    void OnBeforeRender()
    {
        if (copyInput)
        {
            copyInput = false;
            writeText(GUIUtility.systemCopyBuffer);
        }
    }
}