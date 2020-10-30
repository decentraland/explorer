using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.UI;
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
    private Animation[] anims;

    private HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
    private HashSet<Renderer> shadowlessRenderers = new HashSet<Renderer>();
    private HashSet<Material> uniqueMaterials = new HashSet<Material>();
    private Dictionary<Material, List<Renderer>> matToRends = new Dictionary<Material, List<Renderer>>();

    public static bool cullingListDirty = true;
    public static Vector3 lastPlayerPos;

    public Text panel;

    void UpdatePanel()
    {
        int rendererCount = (rs?.Length ?? 0) + (skrs?.Length ?? 0);

        string text = $"Renderer count: {rendererCount}\nHidden count: {hiddenRenderers.Count}\nShadows hidden:{shadowlessRenderers.Count}";
        text += $"\nUnique materials: {uniqueMaterials.Count}";
        panel.text = text;
    }

    void DrawBounds(Bounds b, Color color, float delay = 0)
    {
        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, color, delay);
        Debug.DrawLine(p2, p3, color, delay);
        Debug.DrawLine(p3, p4, color, delay);
        Debug.DrawLine(p4, p1, color, delay);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, color, delay);
        Debug.DrawLine(p6, p7, color, delay);
        Debug.DrawLine(p7, p8, color, delay);
        Debug.DrawLine(p8, p5, color, delay);

        // sides
        Debug.DrawLine(p1, p5, color, delay);
        Debug.DrawLine(p2, p6, color, delay);
        Debug.DrawLine(p3, p7, color, delay);
        Debug.DrawLine(p4, p8, color, delay);
    }

    IEnumerator PopulateRenderersList()
    {
        rs = FindObjectsOfType<Renderer>().Where(x => !(x is SkinnedMeshRenderer)).ToArray();
        yield return null;
        skrs = FindObjectsOfType<SkinnedMeshRenderer>();
        yield return null;
        anims = FindObjectsOfType<Animation>();
    }

    const int MAX_CHECKS_PER_FRAME = 250;

    IEnumerator SetAnimationsCulling()
    {
        Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;
        int counter = 0;

        for (var i = 0; i < anims.Length; i++)
        {
            counter++;

            if (counter == MAX_CHECKS_PER_FRAME)
            {
                counter = 0;
                yield return null;
            }

            Animation anim = anims[i];

            if (anim == null)
                continue;

            Transform t = anim.transform;

            float distance = Vector3.Distance(playerPosition, t.position);

            if (distance > 15)
            {
                anim.cullingType = AnimationCullingType.BasedOnRenderers;
            }
            else
            {
                anim.cullingType = AnimationCullingType.AlwaysAnimate;
            }
        }
    }

    IEnumerator Start()
    {
        profiles = new List<Profile> {rendererProfile, skinnedRendererProfile};
        UpdatePanel();

        CommonScriptableObjects.rendererState.OnChange += (current, previous) => cullingListDirty = true;

        while (true)
        {
            bool shouldCheck = false;

            if (cullingListDirty)
            {
                yield return PopulateRenderersList();
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

            yield return SetAnimationsCulling();

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

                    if (counter == MAX_CHECKS_PER_FRAME)
                    {
                        counter = 0;
                        yield return null;
                    }

                    Renderer r = rsList[i];

                    if (r == null)
                        continue;

                    Transform t = r.transform;
                    Bounds bounds = r.bounds;

                    Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                    float distance = Vector3.Distance(playerPosition, boundingPoint);
                    float size = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

                    float visThreshold = p.rendererVisibilityDistThreshold;
                    float shadowThreshold = p.rendererShadowDistThreshold;

                    bool shouldBeVisible = distance < visThreshold || bounds.Contains(playerPosition);

                    bool isOpaque = true;

                    Material firstMat = r.sharedMaterials[0];

                    if (firstMat != null)
                    {
                        if (firstMat.HasProperty("_ZWrite") &&
                            firstMat.GetFloat("_ZWrite") == 0)
                        {
                            isOpaque = false;
                        }

                        bool hasEmission = false;

                        if (firstMat.HasProperty("_EmissionMap") && firstMat.GetTexture("_EmissionMap") != null)
                            hasEmission = true;

                        if (firstMat.HasProperty("_EmissionColor") && firstMat.GetColor("_EmissionColor") != Color.clear)
                            hasEmission = true;

                        if (hasEmission)
                            shouldBeVisible |= size > p.smallSize / 4;
                    }

                    if (r is SkinnedMeshRenderer)
                    {
                        if (distance > 15)
                            (r as SkinnedMeshRenderer).updateWhenOffscreen = false;
                        else
                            (r as SkinnedMeshRenderer).updateWhenOffscreen = true;
                    }

                    if (isOpaque)
                        shouldBeVisible |= size > p.smallSize;

#if UNITY_EDITOR
                    if (!shouldBeVisible)
                    {
                        DrawBounds(bounds, Color.blue, 1);
                        DrawBounds(new Bounds() {center = boundingPoint, size = Vector3.one}, Color.red, 1);
                    }
#endif
                    bool shouldHaveShadow = distance < shadowThreshold;
                    shouldHaveShadow |= size > p.mediumSize;

                    if (r.forceRenderingOff != !shouldBeVisible)
                    {
                        r.forceRenderingOff = !shouldBeVisible;
                    }

                    if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                        hiddenRenderers.Add(r);

                    var targetMode = shouldHaveShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

                    if (r.shadowCastingMode != targetMode)
                    {
                        r.shadowCastingMode = targetMode;
                    }

                    if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                        shadowlessRenderers.Add(r);
                }
            }

            UpdatePanel();
            yield return null;
        }
    }
}