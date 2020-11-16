using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
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
    [System.Serializable]
    public class CullingControllerProfile
    {
        public float visibleDistanceThreshold;
        public float shadowDistanceThreshold;

        public float emissiveSizeThreshold;
        public float opaqueSizeThreshold;
        public float shadowRendererSizeThreshold;
        public float shadowMapProjectionSizeThreshold;

        public static CullingControllerProfile Lerp(CullingControllerProfile p1, CullingControllerProfile p2, float t)
        {
            //TODO(Brian): Use this to implement settings slider
            return new CullingControllerProfile
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
    public class CullingControllerSettings
    {
        public bool enableObjectCulling = true;
        public bool enableShadowCulling = true;
        public bool enableAnimationCulling = true;

        public float enableAnimationCullingDistance;

        public CullingControllerProfile rendererProfile = new CullingControllerProfile();
        public CullingControllerProfile skinnedRendererProfile = new CullingControllerProfile();
    }

    public interface ICullingController
    {
        void Start();
        void Stop();
        event CullingController.DataReport OnDataReport;
        void SetDirty();

        void SetSettings(CullingControllerSettings settings);
        CullingControllerSettings GetSettings();

        void SetObjectCulling(bool enabled);
        void SetAnimationCulling(bool enabled);
        void SetShadowCulling(bool enabled);
    }

    public class CullingController : ICullingController, IDisposable
    {
        private const float MAX_TIME_BUDGET = 4 / 1000f; // 4 ms

        public CullingControllerSettings settings;

        private List<CullingControllerProfile> profiles = null;

        private HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
        private HashSet<Renderer> shadowlessRenderers = new HashSet<Renderer>();

        public static Vector3 lastPlayerPos;

        public UniversalRenderPipelineAsset urpAsset;

        private bool resetObjectsNextFrame = false;
        private CullingObjectsTracker sceneObjects;
        private Coroutine updateCoroutine;
        private float timeBudgetCount = 0;

        public delegate void DataReport(int rendererCount, int hiddenRendererCount, int hiddenShadowCount);

        public event DataReport OnDataReport;


        public static CullingController Create()
        {
            var settings = new CullingControllerSettings()
            {
                enableAnimationCullingDistance = 7.5f,
                rendererProfile = new CullingControllerProfile
                {
                    visibleDistanceThreshold = 30,
                    shadowDistanceThreshold = 20,
                    emissiveSizeThreshold = 2.5f,
                    opaqueSizeThreshold = 6,
                    shadowRendererSizeThreshold = 10,
                    shadowMapProjectionSizeThreshold = 4
                },
                skinnedRendererProfile = new CullingControllerProfile()
                {
                    visibleDistanceThreshold = 50,
                    shadowDistanceThreshold = 40,
                    emissiveSizeThreshold = 2.5f,
                    opaqueSizeThreshold = 6,
                    shadowRendererSizeThreshold = 5,
                    shadowMapProjectionSizeThreshold = 4,
                }
            };

            return new CullingController(GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset, settings);
        }

        public CullingController(UniversalRenderPipelineAsset urpAsset, CullingControllerSettings settings)
        {
            sceneObjects = new CullingObjectsTracker();
            this.urpAsset = urpAsset;
            this.settings = settings;
        }

        public void Start()
        {
            if (updateCoroutine != null)
                return;

            updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
        }

        public void Stop()
        {
            if (updateCoroutine == null)
                return;

            CoroutineStarter.Stop(updateCoroutine);
            updateCoroutine = null;
        }

        IEnumerator UpdateCoroutine()
        {
            RaiseDataReport();
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
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
                    timeBudgetCount = 0;
                    yield return null;
                    continue;
                }

                if (resetObjectsNextFrame)
                {
                    ResetObjects();
                    resetObjectsNextFrame = false;
                }

                hiddenRenderers.Clear();
                shadowlessRenderers.Clear();

                yield return SetAnimationsCulling();

                int profilesCount = profiles.Count;

                for (var pIndex = 0; pIndex < profilesCount; pIndex++)
                {
                    CullingControllerProfile cullingControllerProfile = profiles[pIndex];
                    Renderer[] renderers = null;

                    if (cullingControllerProfile == settings.rendererProfile)
                        renderers = sceneObjects.renderers;
                    else
                        renderers = sceneObjects.skinnedRenderers;

                    for (var i = 0; i < renderers.Length; i++)
                    {
                        if (timeBudgetCount > MAX_TIME_BUDGET)
                        {
                            timeBudgetCount = 0;
                            yield return null;
                        }

                        Renderer r = renderers[i];

                        if (r == null)
                            continue;

                        float startTime = Time.realtimeSinceStartup;

                        Bounds bounds = r.bounds;
                        Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                        float distance = Vector3.Distance(playerPosition, boundingPoint);

                        bool shouldBeVisible = ShouldBeVisible(distance, bounds, playerPosition, r, cullingControllerProfile);
                        bool shouldHaveShadow = ShouldHaveShadow(distance, bounds, cullingControllerProfile);

                        SetCullingForRenderer(r, shouldBeVisible, shouldHaveShadow);

                        if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                            hiddenRenderers.Add(r);

                        if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                            shadowlessRenderers.Add(r);

                        ShouldUpdateSkinnedWhenOffscreen(r as SkinnedMeshRenderer, settings, distance);

#if UNITY_EDITOR
                        DrawDebugGizmos(shouldBeVisible, bounds, boundingPoint);
#endif
                        timeBudgetCount += Time.realtimeSinceStartup - startTime;
                    }
                }

                RaiseDataReport();
                timeBudgetCount = 0;
                yield return null;
            }
        }

        private void ShouldUpdateSkinnedWhenOffscreen(SkinnedMeshRenderer r, CullingControllerSettings settings, float distance)
        {
            if (r == null)
                return;

            bool finalValue = true;

            if (settings.enableAnimationCulling)
            {
                if (distance > settings.enableAnimationCullingDistance)
                    finalValue = false;
            }

            r.updateWhenOffscreen = finalValue;
        }

        private void SetCullingForRenderer(Renderer r, bool shouldBeVisible, bool shouldHaveShadow)
        {
            var targetMode = shouldHaveShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;

            if (settings.enableObjectCulling)
            {
                if (r.forceRenderingOff != !shouldBeVisible)
                    r.forceRenderingOff = !shouldBeVisible;
            }

            if (settings.enableShadowCulling)
            {
                if (r.shadowCastingMode != targetMode)
                    r.shadowCastingMode = targetMode;
            }
        }

        private bool ShouldBeVisible(float distance, Bounds bounds, Vector3 playerPosition, Renderer r, CullingControllerProfile cullingControllerProfile)
        {
            float size = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

            bool isOpaque = IsOpaque(r);
            bool isEmissive = IsEmissive(r);

            bool shouldBeVisible = distance < cullingControllerProfile.visibleDistanceThreshold || bounds.Contains(playerPosition);

            if (isEmissive) shouldBeVisible |= size > cullingControllerProfile.emissiveSizeThreshold;

            if (isOpaque) shouldBeVisible |= size > cullingControllerProfile.opaqueSizeThreshold;

            return shouldBeVisible;
        }

        private bool ShouldHaveShadow(float distance, Bounds bounds, CullingControllerProfile cullingControllerProfile)
        {
            float size = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

            float shadowSize = bounds.size.magnitude / urpAsset.shadowDistance * urpAsset.mainLightShadowmapResolution;
            bool shouldHaveShadow = distance < cullingControllerProfile.shadowDistanceThreshold;
            shouldHaveShadow |= size > cullingControllerProfile.shadowRendererSizeThreshold;
            shouldHaveShadow &= shadowSize > cullingControllerProfile.shadowMapProjectionSizeThreshold;
            return shouldHaveShadow;
        }

        IEnumerator SetAnimationsCulling()
        {
            if (!settings.enableAnimationCulling)
                yield break;

            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

            int animsLength = sceneObjects.animations.Length;

            for (var i = 0; i < animsLength; i++)
            {
                if (timeBudgetCount > MAX_TIME_BUDGET)
                {
                    timeBudgetCount = 0;
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

                timeBudgetCount += Time.realtimeSinceStartup - startTime;
            }
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


        void ResetObjects()
        {
            foreach (var r in sceneObjects.skinnedRenderers)
            {
                r.updateWhenOffscreen = true;
            }

            foreach (var anim in sceneObjects.animations)
            {
                anim.cullingType = AnimationCullingType.AlwaysAnimate;
            }

            foreach (var r in sceneObjects.renderers)
            {
                r.forceRenderingOff = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public void SetDirty()
        {
            sceneObjects.dirty = true;
        }

        public void SetSettings(CullingControllerSettings settings)
        {
            this.settings = settings;
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
            SetDirty();
        }

        public CullingControllerSettings GetSettings()
        {
            return settings;
        }

        public void SetObjectCulling(bool enabled)
        {
            if (settings.enableObjectCulling == enabled)
                return;

            settings.enableObjectCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        public void SetAnimationCulling(bool enabled)
        {
            if (settings.enableAnimationCulling == enabled)
                return;

            settings.enableAnimationCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        public void SetShadowCulling(bool enabled)
        {
            if (settings.enableShadowCulling == enabled)
                return;

            settings.enableShadowCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        private void RaiseDataReport()
        {
            if (OnDataReport == null)
                return;

            int rendererCount = (sceneObjects.renderers?.Length ?? 0) + (sceneObjects.skinnedRenderers?.Length ?? 0);

            OnDataReport.Invoke(rendererCount, hiddenRenderers.Count, shadowlessRenderers.Count);
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