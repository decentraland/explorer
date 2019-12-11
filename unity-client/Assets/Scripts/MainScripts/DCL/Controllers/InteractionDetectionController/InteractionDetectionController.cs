using UnityEngine;
using DCL.Components;

namespace DCL
{
    public class InteractionDetectionController : MonoBehaviour
    {
        RaycastHit hitInfo;
        OnPointerEventComponent lastHoveredObject = null;

        void Update()
        {
            if (!RenderingController.i.renderingEnabled) return;

            if (Physics.Raycast(PointerEventsController.i.GetRayFromCamera(), out hitInfo, Mathf.Infinity, Configuration.LayerMasks.physicsCastLayerMaskWithoutCharacter))
            {
                OnPointerEventComponent newHoveredObject = hitInfo.collider.GetComponentInParent<OnPointerEventComponent>();

                if (newHoveredObject == lastHoveredObject) return;

                if (newHoveredObject != null)
                {
                    UnhoverLastHoveredObject();

                    newHoveredObject.SetHoverState(true, hitInfo.distance);

                    lastHoveredObject = newHoveredObject;
                }
                else
                {
                    UnhoverLastHoveredObject();
                }
            }
            else
            {
                UnhoverLastHoveredObject();
            }
        }

        void UnhoverLastHoveredObject()
        {
            if (lastHoveredObject == null) return;

            lastHoveredObject.SetHoverState(false);
            lastHoveredObject = null;
        }
    }
}
