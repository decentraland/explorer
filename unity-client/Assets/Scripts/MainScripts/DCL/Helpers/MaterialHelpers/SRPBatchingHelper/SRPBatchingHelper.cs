using UnityEngine;

namespace DCL.Helpers
{

    public static class SRPBatchingHelper
    {
        public static void OptimizeMaterial(Renderer renderer, Material material, int crc)
        {
            int baseQueue;

            if (material.IsKeywordEnabled("_ALPHABLEND_ON"))
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            else
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            int crcBucket = crc % 500;

            //NOTE(Brian): This is to move the rendering of animated stuff on top of the queue, so the SRP batcher
            //             can group all the draw calls.
            if (renderer is SkinnedMeshRenderer)
            {
                material.renderQueue = baseQueue - 500;
            }
            else
            {
                material.renderQueue = baseQueue + crcBucket;
            }

            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");
        }
    }
}
