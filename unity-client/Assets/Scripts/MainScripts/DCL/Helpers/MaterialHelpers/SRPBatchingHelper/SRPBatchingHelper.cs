using System.Text;
using UnityEngine;

namespace DCL.Helpers
{

    public static class SRPBatchingHelper
    {
        static StringBuilder tmpStrBuilder = new StringBuilder(500);
        public static void OptimizeMaterial(Renderer renderer, Material material)
        {
            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");
            material.enableInstancing = true;

            int baseQueue;

            if (material.IsKeywordEnabled("_ALPHABLEND_ON") || material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.Transparent)
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            else if (material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.AlphaTest)
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            else
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            UnityEngine.Rendering.CullMode cullMode = (UnityEngine.Rendering.CullMode)material.GetFloat("_Cull");
            int _ZWrite = (int)material.GetFloat("_ZWrite");
            int _SrcBlend = (int)material.GetFloat("_SrcBlend");
            int _DstBlend = (int)material.GetFloat("_DstBlend");

            var kw = material.shaderKeywords;

            tmpStrBuilder.Clear();

            for (int i = 0; i < kw.Length; i++)
            {
                tmpStrBuilder.Append(kw[i]);
            }

            tmpStrBuilder.Append(cullMode.ToString());
            tmpStrBuilder.Append(_ZWrite.ToString());
            tmpStrBuilder.Append(_SrcBlend.ToString());
            tmpStrBuilder.Append(_DstBlend.ToString());

            int crc = Shader.PropertyToID(tmpStrBuilder.ToString());
            int crcBucket = Mathf.Abs(crc) % 500;

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
