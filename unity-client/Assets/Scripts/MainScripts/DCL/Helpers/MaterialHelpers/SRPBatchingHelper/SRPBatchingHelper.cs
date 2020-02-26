using System.Collections.Generic;
using UnityEngine;

namespace DCL.Helpers
{
    public static class ShaderUtils
    {
        public static readonly int _SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
        public static readonly int _SmoothnessTextureChannel = Shader.PropertyToID("_SmoothnessTextureChannel");
        public static readonly int _SpecColor = Shader.PropertyToID("_SpecColor");
        public static readonly int _GlossMapScale = Shader.PropertyToID("_GlossMapScale");
        public static readonly int _Glossiness = Shader.PropertyToID("_Glossiness");

        public static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
        public static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");
        public static readonly int _Metallic = Shader.PropertyToID("_Metallic");
        public static readonly int _Smoothness = Shader.PropertyToID("_Smoothness");

        public static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");
        public static readonly int _BumpMap = Shader.PropertyToID("_BumpMap");
        public static readonly int _BumpScale = Shader.PropertyToID("_BumpScale");

        public static readonly int _OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        public static readonly int _OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");

        public static readonly int _EmissionMap = Shader.PropertyToID("_EmissionMap");
        public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

        public static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");
        public static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");
        public static readonly int _ZWrite = Shader.PropertyToID("_ZWrite");
        public static readonly int _AlphaClip = Shader.PropertyToID("_AlphaClip");
        public static readonly int _Cull = Shader.PropertyToID("_Cull");

        public static readonly int _SpecularHighlights = Shader.PropertyToID("_SpecularHighlights");
        public static readonly int _EnvironmentReflections = Shader.PropertyToID("_EnvironmentReflections");
    }

    public static class SRPBatchingHelper
    {
        static Dictionary<int, int> crcToQueue = new Dictionary<int, int>();
        public static void OptimizeMaterial(Renderer renderer, Material material)
        {
            //NOTE(Brian): Just enable these keywords so the SRP batcher batches more stuff.
            material.EnableKeyword("_EMISSION");
            material.EnableKeyword("_NORMALMAP");

            material.enableInstancing = true;

            int baseQueue;

            int cullMode = (int)material.GetFloat(ShaderUtils._Cull);
            int zWrite = (int)material.GetFloat(ShaderUtils._ZWrite);

            if (zWrite == 0)
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            else if (material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.AlphaTest)
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            else
                baseQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            if (baseQueue != (int)UnityEngine.Rendering.RenderQueue.Transparent)
            {
                material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                material.SetFloat(ShaderUtils._SpecularHighlights, 1);
                material.SetFloat(ShaderUtils._EnvironmentReflections, 1);
            }

            string appendedKeywords = string.Join("", material.shaderKeywords);
            int crc = Shader.PropertyToID(appendedKeywords);

            if (!crcToQueue.ContainsKey(crc))
                crcToQueue.Add(crc, crcToQueue.Count + 1);

            int offset = ((cullMode + 1) * 100) + (zWrite * 100);

            //NOTE(Brian): This is to move the rendering of animated stuff on top of the queue, so the SRP batcher
            //             can group all the draw calls.
            if (renderer is SkinnedMeshRenderer)
            {
                material.renderQueue = baseQueue;
            }
            else
            {
                material.renderQueue = baseQueue + crcToQueue[crc] + offset;
            }
        }
    }
}
