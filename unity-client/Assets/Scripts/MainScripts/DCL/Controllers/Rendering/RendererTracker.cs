using System;
using System.Collections.Generic;
using UnityEditor.Experimental.TerrainAPI;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// RendererTracker is a wrapper over a CullingGroup object, dedicated to tracking renderers
    /// culling status.
    /// List of responsibilities include:
    ///     - Observer for any number of renderers
    ///     - Event invoking when any of those renderers go over a determined distance or get
    ///       frustum culled.
    /// </summary>
    public class RendererTracker : IDisposable
    {
        private Dictionary<Renderer, int> rendererToIndex = new Dictionary<Renderer, int>();
        private Dictionary<int, Renderer> indexToRenderer = new Dictionary<int, Renderer>();

        BoundingSphere[] boundingSpheres = new BoundingSphere[10000];
        private int boundingSpheresSize = 0;
        private CullingGroup cullingGroup;

        public event System.Action<Renderer, CullingGroupEvent> OnRendererChangeState;

        public void AddRenderer(Renderer r)
        {
            //TODO(Brian): Reference counting to avoid early removal?
            if (rendererToIndex.ContainsKey(r))
                return;

            int index = boundingSpheresSize;
            var bounds = r.bounds;
            boundingSpheres[index] = new BoundingSphere(bounds.center, bounds.size.magnitude);
            boundingSpheresSize++;

            indexToRenderer.Add(index, r);
            rendererToIndex.Add(r, index);
            this.cullingGroup.SetBoundingSphereCount(boundingSpheresSize);
        }

        public void RemoveRenderer(Renderer r)
        {
            if (!rendererToIndex.ContainsKey(r))
                return;

            int index = rendererToIndex[r];

            indexToRenderer.Remove(index);
            rendererToIndex.Remove(r);

            CullingGroup.EraseSwapBack(index, boundingSpheres, ref boundingSpheresSize);
            this.cullingGroup.SetBoundingSphereCount(boundingSpheresSize);
        }

        public RendererTracker(Transform referencePoint, Camera camera, float distanceLimit)
        {
            cullingGroup = new CullingGroup();
            cullingGroup.targetCamera = camera;
            cullingGroup.SetBoundingSpheres(boundingSpheres);
            cullingGroup.SetBoundingDistances(new[] {distanceLimit, float.PositiveInfinity});
            cullingGroup.SetDistanceReferencePoint(referencePoint);
            cullingGroup.SetBoundingSphereCount(0);
            cullingGroup.onStateChanged += OnStateChanged;
        }

        private void OnStateChanged(CullingGroupEvent sphere)
        {
            if (!indexToRenderer.ContainsKey(sphere.index))
                return;

            OnRendererChangeState?.Invoke(indexToRenderer[sphere.index], sphere);
        }

        public void Dispose()
        {
            cullingGroup?.Dispose();
        }
    }
}