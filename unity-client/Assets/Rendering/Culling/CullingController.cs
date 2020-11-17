using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UniversalRenderPipelineAsset = UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;

namespace DCL.Rendering
{
    /// <summary>
    /// 
    /// </summary>
    public class CullingController : ICullingController, IDisposable
    {
        public float maxTimeBudget = 4 / 1000f;

        internal List<CullingControllerProfile> profiles = null;

        public CullingControllerSettings settings;

        private HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
        private HashSet<Renderer> shadowlessRenderers = new HashSet<Renderer>();

        public UniversalRenderPipelineAsset urpAsset;

        private CullingObjectsTracker sceneObjects;
        private Coroutine updateCoroutine;
        private float timeBudgetCount = 0;
        private Vector3 lastPlayerPos;
        private bool resetObjectsNextFrame = false;

        public delegate void DataReport(int rendererCount, int hiddenRendererCount, int hiddenShadowCount);

        public event DataReport OnDataReport;


        public static CullingController Create()
        {
            return new CullingController(
                GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset,
                new CullingControllerSettings()
            );
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

        internal IEnumerator ProcessProfile(CullingControllerProfile profile)
        {
            Renderer[] renderers = null;

            if (profile == settings.rendererProfile)
                renderers = sceneObjects.renderers;
            else
                renderers = sceneObjects.skinnedRenderers;

            for (var i = 0; i < renderers.Length; i++)
            {
                if (timeBudgetCount > maxTimeBudget)
                {
                    timeBudgetCount = 0;
                    yield return null;
                }

                Renderer r = renderers[i];

                if (r == null)
                    continue;

                float startTime = Time.realtimeSinceStartup;

                //NOTE(Brian): Need to retrieve positions every frame to take into account
                //             world repositioning.
                Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

                Bounds bounds = r.bounds;
                Vector3 boundingPoint = bounds.ClosestPoint(playerPosition);
                float distance = Vector3.Distance(playerPosition, boundingPoint);
                bool boundsContainsPlayer = bounds.Contains(playerPosition);
                float viewportSize = (bounds.size.magnitude / distance) * Mathf.Rad2Deg;

                bool isEmissive = IsEmissive(r);
                bool isOpaque = IsOpaque(r);

                bool shouldBeVisible = ShouldBeVisible(profile, viewportSize, distance, boundsContainsPlayer, isOpaque, isEmissive);
                bool shouldHaveShadow = ShouldHaveShadow(profile, viewportSize, bounds.size.magnitude, distance);

                SetCullingForRenderer(r, shouldBeVisible, shouldHaveShadow);

                if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                    hiddenRenderers.Add(r);

                if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                    shadowlessRenderers.Add(r);

                var skmr = r as SkinnedMeshRenderer;

                if (skmr != null)
                {
                    skmr.updateWhenOffscreen = ShouldUpdateSkinnedWhenOffscreen(settings, distance);
                }
#if UNITY_EDITOR
                DrawDebugGizmos(shouldBeVisible, bounds, boundingPoint);
#endif
                timeBudgetCount += Time.realtimeSinceStartup - startTime;
            }
        }

        IEnumerator UpdateCoroutine()
        {
            RaiseDataReport();
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
            CommonScriptableObjects.rendererState.OnChange += (current, previous) => SetDirty();

            while (true)
            {
                if (!CommonScriptableObjects.rendererState.Get())
                {
                    timeBudgetCount = 0;
                    yield return null;
                    continue;
                }

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

                yield return ProcessAnimations();

                hiddenRenderers.Clear();
                shadowlessRenderers.Clear();

                int profilesCount = profiles.Count;

                for (var pIndex = 0; pIndex < profilesCount; pIndex++)
                {
                    CullingControllerProfile profile = profiles[pIndex];
                    yield return ProcessProfile(profile);
                }

                RaiseDataReport();
                timeBudgetCount = 0;
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool ShouldUpdateSkinnedWhenOffscreen(CullingControllerSettings settings, float distance)
        {
            bool finalValue = true;

            if (settings.enableAnimationCulling)
            {
                if (distance > settings.enableAnimationCullingDistance)
                    finalValue = false;
            }

            return finalValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="shouldBeVisible"></param>
        /// <param name="shouldHaveShadow"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="size"></param>
        /// <param name="distance"></param>
        /// <param name="boundsContainsPlayer"></param>
        /// <param name="isOpaque"></param>
        /// <param name="isEmissive"></param>
        /// <returns></returns>
        internal bool ShouldBeVisible(CullingControllerProfile profile, float size, float distance, bool boundsContainsPlayer, bool isOpaque, bool isEmissive)
        {
            bool shouldBeVisible = distance < profile.visibleDistanceThreshold || boundsContainsPlayer;

            if (isEmissive)
                shouldBeVisible |= size > profile.emissiveSizeThreshold;

            if (isOpaque)
                shouldBeVisible |= size > profile.opaqueSizeThreshold;

            return shouldBeVisible;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="viewportSize"></param>
        /// <param name="boundsSize"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        internal bool ShouldHaveShadow(CullingControllerProfile profile, float viewportSize, float boundsSize, float distance)
        {
            float shadowMapRenderSize = boundsSize / urpAsset.shadowDistance * urpAsset.mainLightShadowmapResolution;

            bool shouldHaveShadow = distance < profile.shadowDistanceThreshold;
            shouldHaveShadow |= viewportSize > profile.shadowRendererSizeThreshold;
            shouldHaveShadow &= shadowMapRenderSize > profile.shadowMapProjectionSizeThreshold;
            return shouldHaveShadow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal IEnumerator ProcessAnimations()
        {
            if (!settings.enableAnimationCulling)
                yield break;

            Vector3 playerPosition = CommonScriptableObjects.playerUnityPosition;

            int animsLength = sceneObjects.animations.Length;

            for (var i = 0; i < animsLength; i++)
            {
                if (timeBudgetCount > maxTimeBudget)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
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


        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public void SetDirty()
        {
            sceneObjects.dirty = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void SetSettings(CullingControllerSettings settings)
        {
            this.settings = settings;
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
            SetDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CullingControllerSettings GetSettings()
        {
            return settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        public void SetObjectCulling(bool enabled)
        {
            if (settings.enableObjectCulling == enabled)
                return;

            settings.enableObjectCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        public void SetAnimationCulling(bool enabled)
        {
            if (settings.enableAnimationCulling == enabled)
                return;

            settings.enableAnimationCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        public void SetShadowCulling(bool enabled)
        {
            if (settings.enableShadowCulling == enabled)
                return;

            settings.enableShadowCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RaiseDataReport()
        {
            if (OnDataReport == null)
                return;

            int rendererCount = (sceneObjects.renderers?.Length ?? 0) + (sceneObjects.skinnedRenderers?.Length ?? 0);

            OnDataReport.Invoke(rendererCount, hiddenRenderers.Count, shadowlessRenderers.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shouldBeVisible"></param>
        /// <param name="bounds"></param>
        /// <param name="boundingPoint"></param>
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