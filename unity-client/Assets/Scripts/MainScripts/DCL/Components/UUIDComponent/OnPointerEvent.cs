using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEvent : UUIDComponent<OnPointerEvent.Model>
    {
        public static bool enableInteractionHoverFeedback = true;

        InteractionHoverCanvasController hoverCanvasController;

        [System.Serializable]
        new public class Model : UUIDComponent.Model
        {
            public string button = WebInterface.ACTION_BUTTON.ANY.ToString();
            public string hoverText = "Interact";
            public float distance = 10f;
            public bool showFeedback = true;
        }

        public OnPointerEventColliders pointerEventColliders { get; private set; }

        public override void Initialize(IParcelScene scene, DecentralandEntity entity)
        {
            base.Initialize(scene, entity);

            model = new Model();
            // Create OnPointerEventCollider child
            hoverCanvasController = InteractionHoverCanvasController.i;

            SetEventColliders(entity);

            entity.OnShapeUpdated -= SetEventColliders;
            entity.OnShapeUpdated += SetEventColliders;
        }

        void SetEventColliders(DecentralandEntity entity)
        {
            pointerEventColliders = Utils.GetOrCreateComponent<OnPointerEventColliders>(this.gameObject);
            pointerEventColliders.Initialize(entity);

            //TODO(Brian): Check if this is a bug because it can be called many times on shape update
            pointerEventColliders.refCount++;
        }

        public string GetMeshName(Collider collider)
        {
            return pointerEventColliders.GetMeshName(collider);
        }

        public WebInterface.ACTION_BUTTON GetActionButton()
        {
            switch (model.button)
            {
                case "PRIMARY":
                    return WebInterface.ACTION_BUTTON.PRIMARY;
                case "SECONDARY":
                    return WebInterface.ACTION_BUTTON.SECONDARY;
                case "POINTER":
                    return WebInterface.ACTION_BUTTON.POINTER;
                default:
                    return WebInterface.ACTION_BUTTON.ANY;
            }
        }

        public bool IsVisible()
        {
            if (entity == null)
                return false;

            bool isVisible = false;

            if (this is AvatarOnPointerDown)
                isVisible = true;
            else if (entity.meshesInfo != null && entity.meshesInfo.renderers != null && entity.meshesInfo.renderers.Length > 0)
                isVisible = entity.meshesInfo.renderers[0].enabled;

            return isVisible;
        }

        public void SetHoverState(bool hoverState)
        {
            if (!enableInteractionHoverFeedback || !enabled) return;

            hoverCanvasController.enabled = model.showFeedback;
            if (model.showFeedback)
            {
                if (hoverState)
                    hoverCanvasController.Setup(model.button, model.hoverText, entity);

                hoverCanvasController.SetHoverState(hoverState);
            }
        }

        public bool IsAtHoverDistance(Transform other)
        {
            return model != null && other != null && Vector3.Distance(other.position, transform.position) <= model.distance;
        }

        public bool IsAtHoverDistance(float distance)
        {
            return distance <= model.distance;
        }

        void OnDestroy()
        {
            if (entity != null)
                entity.OnShapeUpdated -= SetEventColliders;

            if (pointerEventColliders != null)
            {
                pointerEventColliders.refCount--;

                if (pointerEventColliders.refCount <= 0)
                {
                    Destroy(pointerEventColliders);
                }
            }
        }
    }
}