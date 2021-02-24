using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIImage : UIShape<UIImageReferencesContainer, UIImage.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public string source;
            public float sourceLeft = 0f;
            public float sourceTop = 0f;
            public float sourceWidth = 1f;
            public float sourceHeight = 1f;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public bool sizeInPixels = true;

            public override bool Equals(object obj)
            {
                return obj is Model model &&
                       base.Equals(obj) &&
                       name == model.name &&
                       parentComponent == model.parentComponent &&
                       visible == model.visible &&
                       opacity == model.opacity &&
                       hAlign == model.hAlign &&
                       vAlign == model.vAlign &&
                       EqualityComparer<UIValue>.Default.Equals(width, model.width) &&
                       EqualityComparer<UIValue>.Default.Equals(height, model.height) &&
                       EqualityComparer<UIValue>.Default.Equals(positionX, model.positionX) &&
                       EqualityComparer<UIValue>.Default.Equals(positionY, model.positionY) &&
                       isPointerBlocker == model.isPointerBlocker &&
                       onClick == model.onClick &&
                       source == model.source &&
                       sourceLeft == model.sourceLeft &&
                       sourceTop == model.sourceTop &&
                       sourceWidth == model.sourceWidth &&
                       sourceHeight == model.sourceHeight &&
                       paddingTop == model.paddingTop &&
                       paddingRight == model.paddingRight &&
                       paddingBottom == model.paddingBottom &&
                       paddingLeft == model.paddingLeft &&
                       sizeInPixels == model.sizeInPixels;
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = 382117979;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(parentComponent);
                hashCode = hashCode * -1521134295 + visible.GetHashCode();
                hashCode = hashCode * -1521134295 + opacity.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(hAlign);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(vAlign);
                hashCode = hashCode * -1521134295 + width.GetHashCode();
                hashCode = hashCode * -1521134295 + height.GetHashCode();
                hashCode = hashCode * -1521134295 + positionX.GetHashCode();
                hashCode = hashCode * -1521134295 + positionY.GetHashCode();
                hashCode = hashCode * -1521134295 + isPointerBlocker.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(onClick);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(source);
                hashCode = hashCode * -1521134295 + sourceLeft.GetHashCode();
                hashCode = hashCode * -1521134295 + sourceTop.GetHashCode();
                hashCode = hashCode * -1521134295 + sourceWidth.GetHashCode();
                hashCode = hashCode * -1521134295 + sourceHeight.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingTop.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingRight.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingBottom.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingLeft.GetHashCode();
                hashCode = hashCode * -1521134295 + sizeInPixels.GetHashCode();
                return hashCode;
            }
        }

        public override string referencesContainerPrefabName => "UIImage";

        DCLTexture dclTexture = null;

        public UIImage(ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int)CLASS_ID.UI_IMAGE_SHAPE;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError("Aborted UIImageShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        Coroutine fetchRoutine;

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            RectTransform parentRecTransform = referencesContainer.GetComponentInParent<RectTransform>();

            // Fetch texture
            if (!string.IsNullOrEmpty(model.source))
            {
                if (dclTexture == null || (dclTexture != null && dclTexture.id != model.source))
                {
                    if (fetchRoutine != null)
                    {
                        scene.StopCoroutine(fetchRoutine);
                        fetchRoutine = null;
                    }

                    IEnumerator fetchIEnum = DCLTexture.FetchTextureComponent(scene, model.source, (downloadedTexture) =>
                    {
                        referencesContainer.image.texture = downloadedTexture.texture;
                        fetchRoutine = null;
                        dclTexture?.DetachFrom(this);
                        dclTexture = downloadedTexture;
                        dclTexture.AttachTo(this);

                        ConfigureUVRect(parentRecTransform);
                    });

                    fetchRoutine = scene.StartCoroutine(fetchIEnum);
                }
            }
            else
            {
                referencesContainer.image.texture = null;
                dclTexture?.DetachFrom(this);
                dclTexture = null;
            }

            referencesContainer.image.enabled = model.visible;
            referencesContainer.image.color = Color.white;

            ConfigureUVRect(parentRecTransform);

            // Apply padding
            referencesContainer.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            referencesContainer.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            referencesContainer.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            referencesContainer.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            Utils.ForceRebuildLayoutImmediate(parentRecTransform);
            return null;
        }

        private void ConfigureUVRect(RectTransform parentRecTransform)
        {
            if (referencesContainer.image.texture == null)
                return;

            // Configure uv rect
            Vector2 normalizedSourceCoordinates = new Vector2(
                model.sourceLeft / referencesContainer.image.texture.width,
                -model.sourceTop / referencesContainer.image.texture.height);

            Vector2 normalizedSourceSize = new Vector2(
                model.sourceWidth * (model.sizeInPixels ? 1f : parentRecTransform.rect.width) /
                referencesContainer.image.texture.width,
                model.sourceHeight * (model.sizeInPixels ? 1f : parentRecTransform.rect.height) /
                referencesContainer.image.texture.height);

            referencesContainer.image.uvRect = new Rect(normalizedSourceCoordinates.x,
                normalizedSourceCoordinates.y + (1 - normalizedSourceSize.y),
                normalizedSourceSize.x,
                normalizedSourceSize.y);
        }

        public override void Dispose()
        {
            dclTexture?.DetachFrom(this);

            if (referencesContainer != null)
                Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}