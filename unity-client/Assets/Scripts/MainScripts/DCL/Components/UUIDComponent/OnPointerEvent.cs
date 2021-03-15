using System.Collections;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEvent : UUIDComponent
    {
        public static bool enableInteractionHoverFeedback = true;

        InteractionHoverCanvasController hoverCanvasController;

        [System.Serializable]
        public new class Model : UUIDComponent.Model
        {
            public string button = WebInterface.ACTION_BUTTON.ANY.ToString();
            public string hoverText = "Interact";
            public float distance = 10f;
            public bool showFeedback = true;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        public OnPointerEventColliders pointerEventColliders { get; private set; }

        public override void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            base.Initialize(scene, entity);

            model = new OnPointerEvent.Model();

            // Create OnPointerEventCollider child
            hoverCanvasController = InteractionHoverCanvasController.i;

            SetEventColliders(entity);

            entity.OnShapeUpdated -= SetEventColliders;
            entity.OnShapeUpdated += SetEventColliders;
        }

        void SetEventColliders(IDCLEntity entity)
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
            Model model = this.model as Model;

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

            Model model = this.model as Model;

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
            Model model = this.model as Model;
            return model != null && other != null && Vector3.Distance(other.position, transform.position) <= model.distance;
        }

        public bool IsAtHoverDistance(float distance)
        {
            Model model = this.model as Model;
            return distance <= model.distance;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            this.model = newModel ?? new OnPointerEvent.Model();
            return null;
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