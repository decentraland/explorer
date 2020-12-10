using System;
using System.Collections.Generic;

public class GIFCacheData : IDisposable
{
    public string url { get; internal set; }
    public string id { get; internal set; }
    public GifFrameData[] textures { get; internal set; }

    public void Dispose()
    {
        if (textures == null)
            return;

        int count = textures.Length;
        for (int i = 0; i < count; i++)
        {
            if (textures[i].texture)
                UnityEngine.Object.Destroy(textures[i].texture);
        }
        textures = null;
    }
}

internal class GIFCache : IDisposable
{
    public enum Status { PENDING, OK, ERROR }

    public int refCount = 0;
    public GIFCacheData data;
    public Status status;

    public void Dispose()
    {
        if (data == null)
            return;

        data.Dispose();
    }
}