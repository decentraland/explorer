﻿using System.Collections.Generic;
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
    public class RendererTracker
    {
        private Dictionary<Renderer, int> rendererToIndex = new Dictionary<Renderer, int>();
        private Dictionary<int, Renderer> indexToRenderer = new Dictionary<int, Renderer>();

        BoundingSphere[] boundingSpheres = new BoundingSphere[10000];
        private int boundingSpheresSize = 0;
        private CullingGroup group;

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
            this.group.SetBoundingSphereCount(boundingSpheresSize);
        }

        public void RemoveRenderer(Renderer r)
        {
            if (!rendererToIndex.ContainsKey(r))
                return;

            int index = rendererToIndex[r];

            indexToRenderer.Remove(index);
            rendererToIndex.Remove(r);

            CullingGroup.EraseSwapBack(index, boundingSpheres, ref boundingSpheresSize);
            this.group.SetBoundingSphereCount(boundingSpheresSize);
        }

        public RendererTracker(Transform referencePoint, Camera camera, float distanceLimit)
        {
            group = new CullingGroup();
            group.targetCamera = camera;
            group.SetBoundingSpheres(boundingSpheres);
            group.SetBoundingDistances(new[] {distanceLimit, float.PositiveInfinity});
            group.SetDistanceReferencePoint(referencePoint);
            group.SetBoundingSphereCount(0);
            group.onStateChanged += OnStateChanged;
        }

        private void OnStateChanged(CullingGroupEvent sphere)
        {
            OnRendererChangeState?.Invoke(indexToRenderer[sphere.index], sphere);
        }
    }
}