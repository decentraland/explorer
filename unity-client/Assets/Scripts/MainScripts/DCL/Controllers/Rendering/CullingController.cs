using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// CullingController is in charge of handling all the specific renderer culling tasks.
    /// List of responsibilities include:
    ///     - Handling animation culling setting depending on distance
    ///     - Any LOD related tasks 
    /// </summary>
    public class CullingController : IDisposable
    {
        private RendererTracker animationCullingTracker;
        private Dictionary<Renderer, Animation> animatorsCache = new Dictionary<Renderer, Animation>();

        public CullingController(Transform playerTransform)
        {
            animationCullingTracker = new RendererTracker(playerTransform, Camera.main, 20f);
            animationCullingTracker.OnRendererChangeState += AnimationCullingTracker_OnChangeState;
        }

        public void AddAnimatedRenderer(Renderer r)
        {
            if (!animatorsCache.ContainsKey(r))
            {
                var anim = r.GetComponentInParent<Animation>();

                if (anim != null)
                {
                    animatorsCache.Add(r, anim);
                    animationCullingTracker.AddRenderer(r);
                }
            }
        }

        public void AddAnimatedRenderers(Renderer[] rs)
        {
            for (int i = 0; i < rs.Length; i++)
            {
                AddAnimatedRenderer(rs[i]);
            }
        }


        public void AddRenderer(Renderer r)
        {
        }

        public void RemoveRenderer(Renderer r)
        {
            animationCullingTracker.RemoveRenderer(r);
        }

        public void AddRenderers(Renderer[] rs)
        {
            for (int i = 0; i < rs.Length; i++)
            {
                AddRenderer(rs[i]);
            }
        }

        public void RemoveRenderers(Renderer[] rs)
        {
            for (int i = 0; i < rs.Length; i++)
            {
                RemoveRenderer(rs[i]);
            }
        }

        public void AddRenderersFromGameObject(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            AddRenderers(rs);
        }

        public void RemoveRenderersFromGameObject(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            RemoveRenderers(rs);
        }

        private void AnimationCullingTracker_OnChangeState(Renderer r, CullingGroupEvent e)
        {
            if (animatorsCache[r] == null)
                return;

            animatorsCache[r].cullingType = e.currentDistance == 0 ? AnimationCullingType.AlwaysAnimate : AnimationCullingType.BasedOnRenderers;
        }

        public void Dispose()
        {
            animationCullingTracker?.Dispose();
        }
    }
}