using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// RenderProfileManifest is used to store and set the current used RenderProfileWorld.
    ///
    /// When a new RenderProfileWorld object is added to the project it must be added here as well.
    /// </summary>
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

        public void Initialize()
        {
            currentProfile = defaultProfile;
            currentProfile.avatarProfile.inWorld.Apply();
            currentProfile.Apply();
        }
    }
}