using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public class UIText : UIShape<UITextReferencesContainer, UIText.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public TextShape.Model textModel;

            public Model()
            {
                textModel = new TextShape.Model();
            }

            public override bool Equals(object obj)
            {
                return obj is Model model &&
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
                       EqualityComparer<TextShape.Model>.Default.Equals(textModel, model.textModel);
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = -719573550;
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
                hashCode = hashCode * -1521134295 + EqualityComparer<TextShape.Model>.Default.GetHashCode(textModel);
                return hashCode;
            }
        }

        public override string referencesContainerPrefabName => "UIText";

        public UIText(ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.UI_TEXT_SHAPE;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError("Aborted UITextShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            if (!scene.isTestScene)
            {
                model = (Model) newModel;
            }

            yield return TextShape.ApplyModelChanges(scene, referencesContainer.text, model.textModel);

            RefreshAll();
        }

        protected override void RefreshDCLSize(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
            {
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();
            }

            if (model.textModel.adaptWidth || model.textModel.adaptHeight)
                referencesContainer.text.ForceMeshUpdate(false);

            Bounds b = referencesContainer.text.textBounds;

            float width, height;

            if (model.textModel.adaptWidth)
            {
                width = b.size.x;
            }
            else
            {
                width = model.width.GetScaledValue(parentTransform.rect.width);
            }

            if (model.textModel.adaptHeight)
            {
                height = b.size.y;
            }
            else
            {
                height = model.height.GetScaledValue(parentTransform.rect.height);
            }

            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override void Dispose()
        {
            if (referencesContainer != null)
                Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}