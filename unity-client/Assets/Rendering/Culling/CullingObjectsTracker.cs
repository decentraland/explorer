using System.Collections;
using System.Linq;
using UnityEngine;

namespace DCL.Rendering
{
    public class CullingObjectsTracker
    {
        public Renderer[] renderers;
        public SkinnedMeshRenderer[] skinnedRenderers;
        public Animation[] animations;

        public bool dirty = true;

        public IEnumerator PopulateRenderersList()
        {
            if (!dirty)
                yield break;

            renderers = Object.FindObjectsOfType<Renderer>()
                .Where(x => !(x is SkinnedMeshRenderer))
                .ToArray();

            yield return null;
            skinnedRenderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();
            yield return null;
            animations = Object.FindObjectsOfType<Animation>();

            dirty = false;
        }
    }
}