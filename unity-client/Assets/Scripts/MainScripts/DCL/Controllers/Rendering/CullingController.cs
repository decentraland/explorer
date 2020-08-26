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
    public class CullingController
    {
        private RendererTracker animationCullingTracker;
        private Dictionary<Renderer, Animation> animatorsCache = new Dictionary<Renderer, Animation>();

        public CullingController(Transform playerTransform)
        {
            animationCullingTracker = new RendererTracker(playerTransform, Camera.main, 20f);
            animationCullingTracker.OnRendererChangeState += AnimationCullingTracker_OnChangeState;
        }

        public void AddRenderer(Renderer r)
        {
            if (r is SkinnedMeshRenderer)
                animationCullingTracker.AddRenderer(r);
        }

        public void RemoveRenderer(Renderer r)
        {
            if (r is SkinnedMeshRenderer)
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
            if (!animatorsCache.ContainsKey(r))
            {
                var anim = r.GetComponentInParent<Animation>();
                animatorsCache[r] = anim;
            }

            animatorsCache[r].cullingType = e.currentDistance == 0 ? AnimationCullingType.AlwaysAnimate : AnimationCullingType.BasedOnRenderers;
        }
    }
}