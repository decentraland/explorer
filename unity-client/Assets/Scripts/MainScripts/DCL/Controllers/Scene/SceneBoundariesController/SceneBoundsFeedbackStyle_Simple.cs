using DCL.Models;
using UnityEngine;
using System.Collections.Generic;

namespace DCL.Controllers
{
    public class SceneBoundsFeedbackStyle_Simple : ISceneBoundsFeedbackStyle
    {
        public void OnRendererExitBounds(Renderer renderer)
        {
            renderer.enabled = false;
        }

        public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (meshesInfo.renderers[0] == null)
                return;

            if (isInsideBoundaries != meshesInfo.renderers[0].enabled && meshesInfo.currentShape.IsVisible())
            {
                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    if (meshesInfo.renderers[i] != null)
                        meshesInfo.renderers[i].enabled = isInsideBoundaries;
                }
            }
        }


        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            List<Material> result = new List<Material>();

            for (int i = 0; i < meshesInfo.renderers.Length; i++)
            {
                result.AddRange(meshesInfo.renderers[i].sharedMaterials);
            }

            return result;
        }
    }
}