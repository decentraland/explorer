using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIContainerRect : UIShape<UIContainerRectReferencesContainer, UIContainerRect.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float thickness = 0f;
            public Color color = Color.clear;
            public bool adaptWidth = false;
            public bool adaptHeight = false;

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
                       thickness == model.thickness &&
                       color.Equals(model.color) &&
                       adaptWidth == model.adaptWidth &&
                       adaptHeight == model.adaptHeight;
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = -1401569357;
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
                hashCode = hashCode * -1521134295 + thickness.GetHashCode();
                hashCode = hashCode * -1521134295 + color.GetHashCode();
                hashCode = hashCode * -1521134295 + adaptWidth.GetHashCode();
                hashCode = hashCode * -1521134295 + adaptHeight.GetHashCode();
                return hashCode;
            }
        }

        public override string referencesContainerPrefabName => "UIContainerRect";

        public UIContainerRect(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_CONTAINER_RECT;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIContainerRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);

            Outline outline = referencesContainer.image.GetComponent<Outline>();

            if (model.thickness > 0f)
            {
                if (outline == null)
                {
                    outline = referencesContainer.image.gameObject.AddComponent<Outline>();
                }

                outline.effectDistance = new Vector2(model.thickness, model.thickness);
            }
            else if (outline != null)
            {
                Object.DestroyImmediate(outline, false);
            }

            return null;
        }

        public override void Dispose()
        {
            if (referencesContainer != null)
                Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}