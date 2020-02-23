using UnityEngine;

namespace DCL.Helpers
{

    public static class SRPBatchingHelper
    {
        public static void OptimizeMaterial(Renderer renderer, Material material, int crc)
        {
            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");

            int baseQueue;

            if (material.IsKeywordEnabled("_ALPHABLEND_ON") || material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.Transparent)
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            else if (material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.AlphaTest)
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            else
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            UnityEngine.Rendering.CullMode cullMode = (UnityEngine.Rendering.CullMode)material.GetFloat("_Cull");
            int crcBucket = crc % 500;

            switch (cullMode)
            {
                case UnityEngine.Rendering.CullMode.Off:
                    crcBucket += 500;
                    break;
                case UnityEngine.Rendering.CullMode.Front:
                    crcBucket += 1000;
                    break;
                case UnityEngine.Rendering.CullMode.Back:
                    crcBucket += 1500;
                    break;
            }

            //NOTE(Brian): This is to move the rendering of animated stuff on top of the queue, so the SRP batcher
            //             can group all the draw calls.
            if (renderer is SkinnedMeshRenderer)
            {
                material.renderQueue = baseQueue - 1000;
            }
            else
            {
                material.renderQueue = baseQueue + crcBucket;
            }
        }
    }
}
