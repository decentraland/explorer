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

        public void ApplyFeedback(DecentralandEntity entity, bool isInsideBoundaries)
        {
            if (entity.meshesInfo.renderers[0] == null)
                return;

            if (isInsideBoundaries != entity.meshesInfo.renderers[0].enabled && entity.meshesInfo.currentShape.IsVisible())
            {
                for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
                {
                    if (entity.meshesInfo.renderers[i] != null)
                        entity.meshesInfo.renderers[i].enabled = isInsideBoundaries;
                }
            }
        }

        public Material[] GetOriginalMaterials(DecentralandEntity entity)
        {
            return null;
        }
    }
}