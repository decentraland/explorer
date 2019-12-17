using DCL.Controllers;
using DCL.Models;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEventComponent : UUIDComponent<OnPointerEventComponent.Model>
    {
        [System.Serializable]
        new public class Model : UUIDComponent.Model
        {
            public int button;
            public string toastText = "Interact";
            public float interactionDistance = 100f;
        }

        Rigidbody rigidBody;
        OnPointerEventColliders pointerEventColliders;
        InteractionHoverCanvasController hoverCanvasController;
        bool beingHovered;

        public override void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            this.entity = entity;
            this.scene = scene;

            if (model == null)
                model = new OnPointerEventComponent.Model();

            model.uuid = uuid;
            model.type = type;

            GameObject hoverCanvasGameObject = Object.Instantiate(Resources.Load("Prefabs/InteractionHoverCanvas"), transform) as GameObject;
            hoverCanvasController = hoverCanvasGameObject.GetComponent<InteractionHoverCanvasController>();
            hoverCanvasController.Setup(model.button, model.toastText);

            Initialize();

            entity.OnShapeUpdated -= OnComponentUpdated;
            entity.OnShapeUpdated += OnComponentUpdated;
        }

        public string GetMeshName(Collider collider)
        {
            return pointerEventColliders.GetMeshName(collider);
        }

        public void Initialize()
        {
            if (!entity.meshRootGameObject) return;

            // we add a rigidbody to the entity's gameobject to have a
            // reference to the entity itself on the RaycastHit
            // so we don't need to search for the parents in order to get
            // the OnPointerEventComponent reference
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                rigidBody = gameObject.AddComponent<Rigidbody>();
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }

            // Create OnPointerEventCollider child
            pointerEventColliders = Utils.GetOrCreateComponent<OnPointerEventColliders>(this.gameObject);
            pointerEventColliders.Initialize(entity);
        }

        void OnComponentUpdated(DecentralandEntity e)
        {
            Initialize();
        }

        protected override void RemoveComponent<T>(DecentralandEntity entity)
        {
            Destroy(hoverCanvasController.gameObject);
        }

        public void SetHoverState(bool isHovered)
        {
            if (isHovered)
                hoverCanvasController.UpdateSizeAndPos();

            if (beingHovered == isHovered) return;

            beingHovered = isHovered;

            if (beingHovered)
                hoverCanvasController.Show();
            else
                hoverCanvasController.Hide();
        }

        public bool IsAtHoverDistance(float distance)
        {
            return distance <= model.interactionDistance;
        }

        void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;

            if (pointerEventColliders)
            {
                pointerEventColliders.refCount--;

                if (pointerEventColliders.refCount <= 0)
                {
                    Destroy(rigidBody);
                    Destroy(pointerEventColliders);
                }
            }
        }
    }
}