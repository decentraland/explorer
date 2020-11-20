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
    /// CullingController has the following responsibilities:
    /// - Hides small renderers (detail objects).
    /// - Disable unneeded shadows.
    /// - Enable/disable animation culling for skinned renderers and animation components.
    /// </summary>
    public class CullingController : ICullingController, IDisposable
    {
        public float maxTimeBudget = 4 / 1000f;

        internal List<CullingControllerProfile> profiles = null;

        private CullingControllerSettings settings;

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

        /// <summary>
        /// Starts culling update coroutine.
        /// The coroutine will keep running until Stop() is called or this class is disposed.
        /// </summary>
        public void Start()
        {
            if (updateCoroutine != null)
                return;

            updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
        }

        /// <summary>
        /// Stops culling update coroutine.
        /// </summary>
        public void Stop()
        {
            if (updateCoroutine == null)
                return;

            CoroutineStarter.Stop(updateCoroutine);
            updateCoroutine = null;
        }

        /// <summary>
        /// Process all sceneObject renderers with the parameters set by the given profile.
        /// 
        /// If profile matches the skinned renderer profile in settings, the skinned renderers are going to be used.
        /// </summary>
        /// <param name="profile">any CullingControllerProfile</param>
        /// <returns>IEnumerator to be yielded.</returns>
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
                float boundsSize = bounds.size.magnitude;
                float viewportSize = (boundsSize / distance) * Mathf.Rad2Deg;

                bool isEmissive = IsEmissive(r);
                bool isOpaque = IsOpaque(r);

                float shadowTexelSize = ComputeShadowMapTexelSize(boundsSize, urpAsset.shadowDistance, urpAsset.mainLightShadowmapResolution);

                bool shouldBeVisible = ShouldBeVisible(profile, viewportSize, distance, boundsContainsPlayer, isOpaque, isEmissive);
                bool shouldHaveShadow = ShouldHaveShadow(profile, viewportSize, distance, shadowTexelSize);

                SetCullingForRenderer(r, shouldBeVisible, shouldHaveShadow);

                if (OnDataReport != null)
                {
                    if (!shouldBeVisible && !hiddenRenderers.Contains(r))
                        hiddenRenderers.Add(r);

                    if (shouldBeVisible && !shouldHaveShadow && !shadowlessRenderers.Contains(r))
                        shadowlessRenderers.Add(r);
                }

                if (r is SkinnedMeshRenderer skr)
                {
                    skr.updateWhenOffscreen = ShouldUpdateSkinnedWhenOffscreen(settings, distance);
                }
#if UNITY_EDITOR
                DrawDebugGizmos(shouldBeVisible, bounds, boundingPoint);
#endif
                timeBudgetCount += Time.realtimeSinceStartup - startTime;
            }
        }

        /// <summary>
        /// Main culling loop. Controlled by Start() and Stop() methods. 
        /// </summary>
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

                if (OnDataReport != null)
                {
                    hiddenRenderers.Clear();
                    shadowlessRenderers.Clear();
                }

                int profilesCount = profiles.Count;

                for (var pIndex = 0; pIndex < profilesCount; pIndex++)
                {
                    yield return ProcessProfile(profiles[pIndex]);
                }

                RaiseDataReport();
                timeBudgetCount = 0;
                yield return null;
            }
        }

        /// <summary>
        /// Sets shadows and visibility for a given renderer.
        /// </summary>
        /// <param name="r">Renderer to be culled</param>
        /// <param name="shouldBeVisible">If false, the renderer visibility will be set to false.</param>
        /// <param name="shouldHaveShadow">If false, the renderer shadow will be toggled off.</param>
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
        /// Sets cullingType to all tracked animation components according to our culling rules.
        /// </summary>
        /// <returns>IEnumerator to be yielded.</returns>
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
        /// Computes the rule used for toggling skinned meshes updateWhenOffscreen param.
        /// Skinned meshes should be always updated if near the camera to avoid false culling positives on screen edges.
        /// </summary>
        /// <param name="settings">Any settings object to use thresholds for computing the rule.</param>
        /// <param name="distance">Mesh distance from camera used for computing the rule.</param>
        /// <returns>True if mesh should be updated when offscreen, false if otherwise.</returns>
        internal static bool ShouldUpdateSkinnedWhenOffscreen(CullingControllerSettings settings, float distance)
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
        /// Computes the rule used for toggling renderers visibility.
        /// </summary>
        /// <param name="profile">Profile used for size and distance thresholds needed for the rule.</param>
        /// <param name="viewportSize">Diagonal viewport size of the renderer.</param>
        /// <param name="distance">Distance to camera of the renderer.</param>
        /// <param name="boundsContainsCamera">Renderer bounds contains camera?</param>
        /// <param name="isOpaque">Renderer is opaque?</param>
        /// <param name="isEmissive">Renderer is emissive?</param>
        /// <returns>True if renderer should be visible, false if otherwise.</returns>
        internal static bool ShouldBeVisible(CullingControllerProfile profile, float viewportSize, float distance, bool boundsContainsCamera, bool isOpaque, bool isEmissive)
        {
            bool shouldBeVisible = distance < profile.visibleDistanceThreshold || boundsContainsCamera;

            if (isEmissive)
                shouldBeVisible |= viewportSize > profile.emissiveSizeThreshold;

            if (isOpaque)
                shouldBeVisible |= viewportSize > profile.opaqueSizeThreshold;

            return shouldBeVisible;
        }

        /// <summary>
        /// Computes the rule used for toggling renderer shadow casting.
        /// </summary>
        /// <param name="profile">Profile used for size and distance thresholds needed for the rule.</param>
        /// <param name="viewportSize">Diagonal viewport size of the renderer</param>
        /// <param name="boundsSize">Bounds size of the renderer computed using bounds.size.magnitude</param>
        /// <param name="distance">Distance from renderer to camera.</param>
        /// <param name="shadowMapSizeTerm">Used for calculating the shadow texel size. Shadow distance * shadow map resolution.</param>
        /// <returns>True if renderer should have shadow, false otherwise</returns>
        internal static bool ShouldHaveShadow(CullingControllerProfile profile, float viewportSize, float distance, float shadowMapTexelSize)
        {
            bool shouldHaveShadow = distance < profile.shadowDistanceThreshold;
            shouldHaveShadow |= viewportSize > profile.shadowRendererSizeThreshold;
            shouldHaveShadow &= shadowMapTexelSize > profile.shadowMapProjectionSizeThreshold;
            return shouldHaveShadow;
        }

        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if its opaque</returns>
        private static bool IsOpaque(Renderer renderer)
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
        /// Determines if the given renderer has emissive material traits.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if the renderer is emissive.</returns>
        private static bool IsEmissive(Renderer renderer)
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
        /// ComputeShadowMapTexelSize computes the shadow-map bounding box diagonal texel size
        /// for the given bounds size.
        /// </summary>
        /// <param name="boundsSize">Diagonal bounds size of the object</param>
        /// <param name="shadowDistance">Shadow distance as set in the quality settings</param>
        /// <param name="shadowMapRes">Shadow map resolution as set in the quality settings (128, 256, etc)</param>
        /// <returns>The computed shadow map diagonal texel size for the object.</returns>
        /// <remarks>
        /// This is calculated by doing the following:
        /// 
        /// - We get the boundsSize to a normalized viewport size.
        /// - We multiply the resulting value by the shadow map resolution.
        /// 
        /// To get the viewport size, we assume the shadow distance value is directly correlated by
        /// the orthogonal projection size used for rendering the shadow map.
        /// 
        /// We can use the bounds size and shadow distance to obtain the normalized shadow viewport
        /// value because both are expressed in world units.
        /// 
        /// After getting the normalized size, we scale it by the shadow map resolution to get the
        /// diagonal texel size of the bounds shadow.
        /// 
        /// This leaves us with:
        ///     <c>shadowTexelSize = boundsSize / shadow dist * shadow res</c>
        /// 
        /// This is a lazy approximation and most likely will need some refinement in the future.
        /// </remarks>
        internal static float ComputeShadowMapTexelSize(float boundsSize, float shadowDistance, float shadowMapRes)
        {
            return boundsSize / shadowDistance * shadowMapRes;
        }


        /// <summary>
        /// Reset all tracked renderers properties. Needed when toggling or changing settings.
        /// </summary>
        internal void ResetObjects()
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
        /// Sets the scene objects dirtiness.
        /// In the next update iteration, all the scene objects are going to be gathered.
        /// This method has performance impact. 
        /// </summary>
        public void SetDirty()
        {
            sceneObjects.dirty = true;
        }

        /// <summary>
        /// Set settings. This will dirty the scene objects and has performance impact.
        /// </summary>
        /// <param name="settings">Settings to be set</param>
        public void SetSettings(CullingControllerSettings settings)
        {
            this.settings = settings;
            profiles = new List<CullingControllerProfile> {settings.rendererProfile, settings.skinnedRendererProfile};
            SetDirty();
        }

        /// <summary>
        /// Get current settings copy. If you need to modify it, you must set them via SetSettings afterwards.
        /// </summary>
        /// <returns>Current settings object copy.</returns>
        public CullingControllerSettings GetSettings()
        {
            return settings.Clone();
        }

        /// <summary>
        /// Enable or disable object visibility culling.
        /// </summary>
        /// <param name="enabled">If disabled, object visibility culling will be toggled.
        /// </param>
        public void SetObjectCulling(bool enabled)
        {
            if (settings.enableObjectCulling == enabled)
                return;

            settings.enableObjectCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        /// <summary>
        /// Enable or disable animation culling.
        /// </summary>
        /// <param name="enabled">If disabled, animation culling will be toggled.</param>
        public void SetAnimationCulling(bool enabled)
        {
            if (settings.enableAnimationCulling == enabled)
                return;

            settings.enableAnimationCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        /// <summary>
        /// Enable or disable shadow culling
        /// </summary>
        /// <param name="enabled">If disabled, no shadows will be toggled.</param>
        public void SetShadowCulling(bool enabled)
        {
            if (settings.enableShadowCulling == enabled)
                return;

            settings.enableShadowCulling = enabled;
            resetObjectsNextFrame = true;
            SetDirty();
        }

        /// <summary>
        /// Fire the DataReport event. This will be useful for showing stats in a debug panel.
        /// </summary>
        private void RaiseDataReport()
        {
            if (OnDataReport == null)
                return;

            int rendererCount = (sceneObjects.renderers?.Length ?? 0) + (sceneObjects.skinnedRenderers?.Length ?? 0);

            OnDataReport.Invoke(rendererCount, hiddenRenderers.Count, shadowlessRenderers.Count);
        }

        /// <summary>
        /// Draw debug gizmos on the scene view.  
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