using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    [CreateAssetMenu(menuName = "DCL/Rendering/Render Profile Manifest", fileName = "RenderProfileManifest", order = 0)]
    public class RenderProfileManifest : ScriptableObject
    {
        private static RenderProfileManifest instance;
        public static RenderProfileManifest i => GetOrLoad(ref instance, "Render Profile Manifest");

        public RenderProfileWorld defaultProfile;
        public RenderProfileWorld halloweenProfile;
        public static RenderProfileWorld currentProfile;

        internal static T GetOrLoad<T>(ref T variable, string path) where T : Object
        {
            if (variable == null)
            {
                variable = Resources.Load<T>(path);
            }

            return variable;
        }
    }
}