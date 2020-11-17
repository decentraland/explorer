using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    public static class SRPBatchingHelper
    {
        static Dictionary<int, int> crcToQueue = new Dictionary<int, int>();
        static Dictionary<int, int> textureOffsets = new Dictionary<int, int>();
        static List<int> textureIds = new List<int>();

        public static void OptimizeMaterial(Material material)
        {
            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
            material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            material.DisableKeyword("VERTEX_COLOR_ON");

            material.SetFloat(ShaderUtils.SpecularHighlights, 1);
            material.SetFloat(ShaderUtils.EnvironmentReflections, 1);

            material.enableInstancing = false;

            if (material.HasProperty(ShaderUtils.ZWrite))
            {
                int zWrite = (int) material.GetFloat(ShaderUtils.ZWrite);

                //NOTE(Brian): for transparent meshes skip further variant optimization.
                //             Transparency needs clip space z sorting to be displayed correctly.
                if (zWrite == 0)
                {
                    material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                    return;
                }
            }

            int cullMode = (int) UnityEngine.Rendering.CullMode.Off;

            if (material.HasProperty(ShaderUtils.Cull))
            {
                cullMode = 2 - (int) material.GetFloat(ShaderUtils.Cull);
            }

            int baseQueue;

            if (material.renderQueue == (int) UnityEngine.Rendering.RenderQueue.AlphaTest)
                baseQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry + 600;
            else
                baseQueue = (int) UnityEngine.Rendering.RenderQueue.Geometry;


            //NOTE(Brian): This guarantees grouping calls by same shader keywords. Needed to take advantage of SRP batching.
            string appendedKeywords = material.shader.name + string.Join("", material.shaderKeywords);

            //string textureHash = "";
            material.GetTexturePropertyNameIDs(textureIds);
            for (var i = 0; i < textureIds.Count; i++)
            {
                material.SetTexture(textureIds[i], Texture2D.whiteTexture);
                //textureHash += material.GetTexture(textureIds[i]).GetNativeTexturePtr();
            }

            //int textureGroupCrc = Shader.PropertyToID(textureHash);

            //if (!textureOffsets.ContainsKey(textureGroupCrc))
            //    textureOffsets.Add(textureGroupCrc, textureOffsets.Count + 1);

            int crc = Shader.PropertyToID(appendedKeywords);

            if (!crcToQueue.ContainsKey(crc))
                crcToQueue.Add(crc, crcToQueue.Count + 1);

            //NOTE(Brian): we use 0, 100, 200 to group calls by culling mode (must group them or batches will break).
            int queueOffset = (cullMode + 1) * 150;
            //int textureOffset = textureOffsets[textureGroupCrc] * 10;
            material.renderQueue = baseQueue + crcToQueue[crc] + queueOffset;
        }
    }
}