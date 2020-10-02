using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RenderProfiles
{
    private static RenderProfileAvatar renderProfileAvatarValue;
    public static RenderProfileAvatar renderProfileAvatar => GetOrLoad(ref renderProfileAvatarValue, "RenderProfileAvatar");

    internal static T GetOrLoad<T>(ref T variable, string path) where T : Object
    {
        if (variable == null)
        {
            variable = Resources.Load<T>(path);
        }

        return variable;
    }
}