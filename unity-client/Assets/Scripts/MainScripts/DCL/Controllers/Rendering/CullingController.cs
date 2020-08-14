using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
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

    private Renderer[] rs;
    private SkinnedMeshRenderer[] skrs;
    public static bool cullingListDirty = true;
    public static Vector3 lastPlayerPos;

    public bool paused = false;

    IEnumerator Start()
    {
        Debug.Log("Press H to optimize.");
        profiles = new List<Profile> {rendererProfile, skinnedRendererProfile};

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                paused = !paused;
                Debug.Log("Optimizer Enabled? = " + !paused);
            }

            if (paused)
            {
                yield return null;
                continue;
            }

            if (cullingListDirty)
            {
                rs = FindObjectsOfType<Renderer>().Where(x => !(x is SkinnedMeshRenderer)).ToArray();
                yield return null;
                skrs = FindObjectsOfType<SkinnedMeshRenderer>();
                yield return null;
                cullingListDirty = false;
            }

            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

            if (playerPosition == lastPlayerPos)
            {
                yield return null;
                continue;
            }

            lastPlayerPos = playerPosition;

            foreach (Profile p in profiles)
            {
                Renderer[] rsList = null;

                if (p == rendererProfile)
                    rsList = rs;
                else
                    rsList = skrs;

                int counter = 0;

                for (var i = 0; i < rsList.Length; i++)
                {
                    counter++;

                    if (counter == 2000)
                    {
                        counter = 0;
                        yield return null;
                    }

                    Renderer r = rsList[i];

                    if (r == null)
                        continue;

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

            yield return null;
        }
    }
}