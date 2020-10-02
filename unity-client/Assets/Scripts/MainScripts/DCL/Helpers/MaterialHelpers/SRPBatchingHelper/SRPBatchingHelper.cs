using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    public static class SRPBatchingHelper
    {
        static Dictionary<int, int> crcToQueue = new Dictionary<int, int>();

        public static void OptimizeMaterial(Renderer renderer, Material material)
        {
            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");

            material.enableInstancing = true;

            int zWrite = (int) material.GetFloat(ShaderUtils._ZWrite);

            //NOTE(Brian): for transparent meshes skip further variant optimization.
            //             Transparency needs clip space z sorting to be displayed correctly.
            if (zWrite == 0)
            {
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                return;
            }

            int cullMode = (int) material.GetFloat(ShaderUtils._Cull);

            int baseQueue;

            if (material.renderQueue == (int) UnityEngine.Rendering.RenderQueue.AlphaTest)
                baseQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
            else
                baseQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry;

            material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            material.SetFloat(ShaderUtils._SpecularHighlights, 1);
            material.SetFloat(ShaderUtils._EnvironmentReflections, 1);

            //NOTE(Brian): This guarantees grouping calls by same shader keywords. Needed to take advantage of SRP batching.
            string appendedKeywords = string.Join("", material.shaderKeywords);
            int crc = Shader.PropertyToID(appendedKeywords);

            if (!crcToQueue.ContainsKey(crc))
                crcToQueue.Add(crc, crcToQueue.Count + 1);

            //NOTE(Brian): This is to move the rendering of animated stuff on top of the queue, so the SRP batcher
            //             can group all the draw calls.
            if (renderer is SkinnedMeshRenderer)
            {
                material.renderQueue = baseQueue;
            }
            else
            {
                //NOTE(Brian): we use 0, 100, 200 to group calls by culling mode (must group them or batches will break).
                int queueOffset = (cullMode + 1) * 100;

                material.renderQueue = baseQueue + crcToQueue[crc] + queueOffset;
            }
        }
    }
}