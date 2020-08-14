using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Object = System.Object;

public class CullingController : MonoBehaviour
{
    [System.Serializable]
    public class Profile
    {
        public float mediumSize = 300;
        public float smallSize = 100;
        public float rendererVisibilityDistThreshold = 64;
        public float rendererShadowDistThreshold = 45;
    }

    public Profile rendererProfile = new Profile();
    public Profile skinnedRendererProfile = new Profile();

    private List<Profile> profiles = null;

    void Start()
    {
        Debug.Log("Press H to optimize.");
        profiles = new List<Profile> {rendererProfile, skinnedRendererProfile};
    }


    private Renderer[] rs;
    private SkinnedMeshRenderer[] skrs;
    public static bool cullingListDirty = true;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.H))
            return;

        Debug.Log("Optimizing renderers...");
        float time = Time.realtimeSinceStartup;

        if (cullingListDirty)
        {
            rs = FindObjectsOfType<Renderer>().Where(x => !(x is SkinnedMeshRenderer)).ToArray();
            skrs = FindObjectsOfType<SkinnedMeshRenderer>();
            cullingListDirty = false;
        }

        Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

        foreach (Profile p in profiles)
        {
            Renderer[] rsList = null;

            if (p == rendererProfile)
                rsList = rs;
            else
                rsList = skrs;

            for (var i = 0; i < rsList.Length; i++)
            {
                Renderer r = rsList[i];
                Transform t = r.transform;
                float distance = Vector3.Distance(playerPosition, r.bounds.center + t.position);
                float size = (r.bounds.size.magnitude / distance) * Mathf.Rad2Deg;

                float visThreshold = p.rendererVisibilityDistThreshold;
                float shadowThreshold = p.rendererShadowDistThreshold;

                bool shouldBeVisible = distance < visThreshold;
                bool isOpaque = r.materials[0].renderQueue < 3000;

                if (isOpaque)
                    shouldBeVisible |= distance >= visThreshold && size > p.smallSize;

                bool shouldHaveShadow = distance < shadowThreshold;
                shouldHaveShadow |= distance >= shadowThreshold && size > p.mediumSize;

                if (r.enabled != shouldBeVisible)
                    r.enabled = shouldBeVisible;

                var targetMode = shouldHaveShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

                if (r.shadowCastingMode != targetMode)
                    r.shadowCastingMode = targetMode;
            }
        }

        time = Time.realtimeSinceStartup - time;

        Debug.Log($"Optimizing renderers... DONE. Time: {time * 1000}ms");
    }
}