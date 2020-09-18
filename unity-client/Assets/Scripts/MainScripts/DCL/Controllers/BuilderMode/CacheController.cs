using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacheController : MonoBehaviour
{
    private static CacheController instanceValue;
    public static CacheController i
    {
        get
        {
            if (instanceValue == null)
            {
                instanceValue = new GameObject("_CacheController").AddComponent<CacheController>();
            }

            return instanceValue;
        }
    }

    Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
    Dictionary<string, System.Action<Sprite>> callbackDictionary = new Dictionary<string, System.Action<Sprite>>();


    public void GetSprite(string url, System.Action<Sprite> callback)
    {
        if (cachedSprites.ContainsKey(url)) callback?.Invoke(cachedSprites[url]);
        else
        {
            if (callbackDictionary.ContainsKey(url))
            {
                callbackDictionary[url] = callback;
                return;
            }
            callbackDictionary.Add(url, callback);
            ExternalCallsController.i.GetContentAsByteArray(url, SetSprite);
        }
        
    }
    public void ClearCache()
    {
        cachedSprites.Clear();
    }


    void SetSprite(string url,byte[] spriteByteArray)
    {
        try
        {
            Texture2D tex2D = new Texture2D(128, 128);
            tex2D.LoadImage(spriteByteArray);
            Sprite newSprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0), 72);


            cachedSprites.Add(url, newSprite);

            callbackDictionary[url].Invoke(newSprite);
            callbackDictionary.Remove(url);
        }
        catch (Exception e)
        {
            Debug.Log("Error loading sprite " + e);
        }
    }

}
