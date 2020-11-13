using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UniversalRenderPipelineAsset = UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;

// Culling logic:
//
//    - Keep dirtyness of renderers
//    - Iterate new renderers every X time, when the player moves or they are dirty
//
//    SkinnedMeshRenderers:
//    - If they are far, set updateWhenOffscreen accordingly
//    - different size values than other renderers
//
//    All renderers visibility logic:
//    - Is less than X distance? -> should be visible
//    - player is inside the renderer bounds? -> should be visible
//    - is emissive and is very small size? -> should be visible
//    - is opaque and is small size? -> should be visible
//
//    All renderers shadow logic:
//    - Is less than Y distance? -> should have shadow
//    - Is medium size? -> should have shadow
//    - shadowmap shadow size is less than 4 texels? -> shouldn't have shadow
//    
//    
//    Sizes:
//    - Distance threshold
//    - Emissive renderer size culling
//    - Opaque renderer size culling
//    - Shadow renderer size culling
//    - ShadowMap projection size threshold

namespace DCL.Rendering
{
    public interface ICullingController
    {
        event CullingController.DataReport OnDataReport;
        void SetDirty();
    }

    public class CullingController : ICullingController
    {
        private const float MAX_TIME_BUDGET = 1 / 1000f; // 1 ms

        [System.Serializable]
        public class Profile
        {
            public float visibleDistanceThreshold;
            public float shadowDistanceThreshold;

            public float emissiveSizeThreshold;
            public float opaqueSizeThreshold;
            public float shadowRendererSizeThreshold;
            public float shadowMapProjectionSizeThreshold;

            public static Profile Lerp(Profile p1, Profile p2, float t)
            {
                //TODO(Brian): Use this to implement settings slider
                return new Profile
                {
                    visibleDistanceThreshold = Mathf.Lerp(p1.visibleDistanceThreshold, p2.visibleDistanceThreshold, t),
                    shadowDistanceThreshold = Mathf.Lerp(p1.shadowDistanceThreshold, p2.shadowDistanceThreshold, t),
                    emissiveSizeThreshold = Mathf.Lerp(p1.emissiveSizeThreshold, p2.emissiveSizeThreshold, t),
                    opaqueSizeThreshold = Mathf.Lerp(p1.opaqueSizeThreshold, p2.opaqueSizeThreshold, t),
                    shadowRendererSizeThreshold = Mathf.Lerp(p1.shadowRendererSizeThreshold, p2.shadowRendererSizeThreshold, t),
                    shadowMapProjectionSizeThreshold = Mathf.Lerp(p1.shadowMapProjectionSizeThreshold, p2.shadowMapProjectionSizeThreshold, t)
                };
            }
        }

        [System.Serializable]
        public class Settings
        {
            public float enableAnimationCullingDistance;
            public Profile rendererProfile = new Profile();
            public Profile skinnedRendererProfile = new Profile();
        }

        public Settings settings;

        private List<Profile> profiles = null;

        private HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
        private HashSet<Renderer> shadowlessRenderers = new HashSet<Renderer>();

        public static Vector3 lastPlayerPos;

        public UniversalRenderPipelineAsset urpAsset;
        private CullingObjectsTracker sceneObjects;
        float timer = 0;

        public delegate void DataReport(int rendererCount, int hiddenRendererCount, int hiddenShadowCount);

        public event DataReport OnDataReport;

        public void SetDirty()
        {
            sceneObjects.dirty = true;
        }

        public static CullingController Create()
        {
            var settings = new Settings()
            {
                enableAnimationCullingDistance = 15,
                rendererProfile = new Profile
                {
                    visibleDistanceThreshold = 30,
                    shadowDistanceThreshold = 20,
                    emissiveSizeThreshold = 1,
                    opaqueSizeThreshold = 4,
                    shadowRendererSizeThreshold = 10,
                    shadowMapProjectionSizeThreshold = 4
                },
                skinnedRendererProfile = new Profile()
                {
                    visibleDistanceThreshold = 50,
                    shadowDistanceThreshold = 40,
                    emissiveSizeThreshold = 1,
                    opaqueSizeThreshold = 4,
                    shadowRendererSizeThreshold = 5,
                    shadowMapProjectionSizeThreshold = 4,
                }
            };

            return new CullingController(GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset, settings);
        }

        public CullingController(UniversalRenderPipelineAsset urpAsset, Settings settings)
        {
            sceneObjects = new CullingObjectsTracker();
            this.urpAsset = urpAsset;
            this.settings = settings;
            CoroutineStarter.Start(UpdateCoroutine());
        }

        IEnumerator UpdateCoroutine()
        {
            RaiseDataReport();
            profiles = new List<Profile> {settings.rendererProfile, settings.skinnedRendererProfile};
            CommonScriptableObjects.rendererState.OnChange += (current, previous) => SetDirty();

            while (true)
            {
                bool shouldCheck = sceneObjects.dirty;

                yield return sceneObjects.PopulateRenderersList();

                Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

                if (Vector3.Distance(playerPosition, lastPlayerPos) > 1.0f)
                {
                    //NOTE(Brian): If player moves, we should always check.
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

                int profilesCount = profiles.Count;

                for (var pIndex = 0; pIndex < profilesCount; pIndex++)
                {
                    Profile profile = profiles[pIndex];
                    Renderer[] renderers = null;

                    if (profile == settings.rendererProfile)
                        renderers = sceneObjects.renderers;
                    else
                        renderers = sceneObjects.skinnedRenderers;

                    for (var i = 0; i < renderers.Length; i++)
                    {
                        if (timer > MAX_TIME_BUDGET)
                        {
                            timer = 0;
                            yield return null;
                        }

                        Renderer r = renderers[i];

                        if (r == null)
                            continue;

                        float startTime = Time.realtimeSinceStartup;

                        Bounds bounds = r.bounds;
                        Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                        float distance = Vector3.Distance(playerPosition, boundingPoint);

                        bool shouldBeVisible = ShouldBeVisible(distance, bounds, playerPosition, r, profile);
                        bool shouldHaveShadow = ShouldHaveShadow(distance, bounds, profile);

                        SetCullingForRenderer(r, shouldBeVisible, shouldHaveShadow);

                        if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                            hiddenRenderers.Add(r);

                        if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                            shadowlessRenderers.Add(r);

                        var skr = r as SkinnedMeshRenderer;

                        if (skr != null)
                        {
                            if (distance > settings.enableAnimationCullingDistance)
                                skr.updateWhenOffscreen = false;
                            else
                                skr.updateWhenOffscreen = true;
                        }

#if UNITY_EDITOR
                        DrawDebugGizmos(shouldBeVisible, bounds, boundingPoint);
#endif
                        timer += Time.realtimeSinceStartup - startTime;
                    }
                }

                RaiseDataReport();
                yield return null;
            }
        }

        private void SetCullingForRenderer(Renderer r, bool shouldBeVisible, bool shouldHaveShadow)
        {
            var targetMode = shouldHaveShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if (r.forceRenderingOff != !shouldBeVisible)
                r.forceRenderingOff = !shouldBeVisible;

            if (r.shadowCastingMode != targetMode)
                r.shadowCastingMode = targetMode;
        }

        private bool ShouldBeVisible(float distance, Bounds bounds, Vector3 playerPosition, Renderer r, Profile profile)
        {
            float size = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

            bool isOpaque = IsOpaque(r);
            bool isEmissive = IsEmissive(r);

            bool shouldBeVisible = distance < profile.visibleDistanceThreshold || bounds.Contains(playerPosition);

            if (isEmissive) shouldBeVisible |= size > profile.emissiveSizeThreshold;

            if (isOpaque) shouldBeVisible |= size > profile.opaqueSizeThreshold;

            return shouldBeVisible;
        }

        private bool ShouldHaveShadow(float distance, Bounds bounds, Profile profile)
        {
            float size = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

            float shadowSize = bounds.size.magnitude / urpAsset.shadowDistance * urpAsset.mainLightShadowmapResolution;
            bool shouldHaveShadow = distance < profile.shadowDistanceThreshold;
            shouldHaveShadow |= size > profile.shadowRendererSizeThreshold;
            shouldHaveShadow &= shadowSize > profile.shadowMapProjectionSizeThreshold;
            return shouldHaveShadow;
        }

        IEnumerator SetAnimationsCulling()
        {
            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

            int animsLength = sceneObjects.animations.Length;

            for (var i = 0; i < animsLength; i++)
            {
                if (timer > MAX_TIME_BUDGET)
                {
                    timer = 0;
                    yield return null;
                }

                Animation anim = sceneObjects.animations[i];

                if (anim == null)
                    continue;

                float startTime = Time.realtimeSinceStartup;
                Transform t = anim.transform;

                float distance = Vector3.Distance(playerPosition, t.position);

                if (distance > settings.enableAnimationCullingDistance)
                    anim.cullingType = AnimationCullingType.BasedOnRenderers;
                else
                    anim.cullingType = AnimationCullingType.AlwaysAnimate;

                timer += Time.realtimeSinceStartup - startTime;
            }
        }

        private void RaiseDataReport()
        {
            if (OnDataReport == null)
                return;

            int rendererCount = (sceneObjects.renderers?.Length ?? 0) + (sceneObjects.skinnedRenderers?.Length ?? 0);
            OnDataReport.Invoke(rendererCount, hiddenRenderers.Count, shadowlessRenderers.Count);
        }

        private bool IsOpaque(Renderer renderer)
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return true;

            if (firstMat.HasProperty(ShaderUtils.ZWrite) &&
                (int) firstMat.GetFloat(ShaderUtils.ZWrite) == 0)
            {
                return false;
            }

            return true;
        }

        private bool IsEmissive(Renderer renderer)
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return false;

            if (firstMat.HasProperty(ShaderUtils.EmissionMap) && firstMat.GetTexture(ShaderUtils.EmissionMap) != null)
                return true;

            if (firstMat.HasProperty(ShaderUtils.EmissionColor) && firstMat.GetColor(ShaderUtils.EmissionColor) != Color.clear)
                return true;

            return false;
        }

        private static void DrawDebugGizmos(bool shouldBeVisible, Bounds bounds, Vector3 boundingPoint)
        {
            if (!shouldBeVisible)
            {
                CullingControllerUtils.DrawBounds(bounds, Color.blue, 1);
                CullingControllerUtils.DrawBounds(new Bounds() {center = boundingPoint, size = Vector3.one}, Color.red, 1);
            }
        }
    }
}