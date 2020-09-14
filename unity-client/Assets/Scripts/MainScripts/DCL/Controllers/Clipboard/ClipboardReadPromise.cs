using System;
using UnityEngine;

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
    
    internal ClipboardReadPromise()
    {
    }
}