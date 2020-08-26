using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
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

    private HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
    private HashSet<Renderer> shadowlessRenderers = new HashSet<Renderer>();

    public static bool cullingListDirty = true;
    public static Vector3 lastPlayerPos;

    public TextMeshProUGUI panel;

    public bool paused = false;

    void UpdatePanel()
    {
        string pausedString = paused ? "OFF" : "ON";
        int rendererCount = (rs?.Length ?? 0) + (skrs?.Length ?? 0);
        panel.text = $"Culling: {pausedString} (H = toggle)\nRenderer count: {rendererCount}\nHidden count: {hiddenRenderers.Count}\nShadows hidden:{shadowlessRenderers.Count}";
    }

    IEnumerator Start()
    {
        profiles = new List<Profile> {rendererProfile, skinnedRendererProfile};
        UpdatePanel();

        CommonScriptableObjects.rendererState.OnChange += (current, previous) => cullingListDirty = true;

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                paused = !paused;
                UpdatePanel();
            }

            if (paused)
            {
                yield return null;
                continue;
            }

            bool shouldCheck = false;

            if (cullingListDirty)
            {
                rs = FindObjectsOfType<Renderer>().Where(x => !(x is SkinnedMeshRenderer)).ToArray();
                yield return null;
                skrs = FindObjectsOfType<SkinnedMeshRenderer>();
                yield return null;
                cullingListDirty = false;
                shouldCheck = true;
            }

            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

            if (Vector3.Distance(playerPosition, lastPlayerPos) > 1.0f)
            {
                shouldCheck = true;
                lastPlayerPos = playerPosition;
            }

            if (!shouldCheck)
            {
                yield return null;
                continue;
            }

            hiddenRenderers.Clear();
            shadowlessRenderers.Clear();

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

                    if (counter == 500)
                    {
                        counter = 0;
                        yield return null;
                    }

                    Renderer r = rsList[i];

                    if (r == null)
                        continue;

                    Transform t = r.transform;
                    Bounds bounds = r.bounds;
                    bounds.center += t.position;

                    Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                    float distance = Vector3.Distance(playerPosition, boundingPoint);
                    float size = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

                    float visThreshold = p.rendererVisibilityDistThreshold;
                    float shadowThreshold = p.rendererShadowDistThreshold;

                    bool shouldBeVisible = distance < visThreshold || bounds.Contains(playerPosition);
                    bool isOpaque = r.materials[0].renderQueue < 3000;

                    if (isOpaque)
                        shouldBeVisible |= size > p.smallSize;

                    bool shouldHaveShadow = distance < shadowThreshold;
                    shouldHaveShadow |= distance >= shadowThreshold && size > p.mediumSize;

                    if (r.enabled != shouldBeVisible)
                    {
                        r.enabled = shouldBeVisible;
                    }

                    if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                        hiddenRenderers.Add(r);

                    var targetMode = shouldHaveShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

                    if (r.shadowCastingMode != targetMode)
                    {
                        r.shadowCastingMode = targetMode;
                    }

                    if (!shouldHaveShadow && !shadowlessRenderers.Contains(r))
                        shadowlessRenderers.Add(r);
                }
            }

            UpdatePanel();
            yield return null;
        }
    }
}