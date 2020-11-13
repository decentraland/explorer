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

            renderers = UnityEngine.Object.FindObjectsOfType<Renderer>()
                .Where(x => !(x is SkinnedMeshRenderer))
                .ToArray();

            yield return null;
            skinnedRenderers = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
            yield return null;
            animations = UnityEngine.Object.FindObjectsOfType<Animation>();

            dirty = false;
        }
    }
}