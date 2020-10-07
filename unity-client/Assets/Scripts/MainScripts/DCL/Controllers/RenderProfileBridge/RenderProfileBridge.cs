using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class RenderProfileBridge : MonoBehaviour
{
    public enum ID
    {
        DEFAULT,
        HALLOWEEN,
    }

    public static RenderProfileBridge i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    public void SetRenderProfile(ID id)
    {
        switch (id)
        {
            case ID.DEFAULT:
                RenderProfileManifest.currentProfile = RenderProfileManifest.i.defaultProfile;
                break;
            case ID.HALLOWEEN:
                RenderProfileManifest.currentProfile = RenderProfileManifest.i.halloweenProfile;
                break;
        }

        RenderProfileManifest.currentProfile.Apply();
    }
}