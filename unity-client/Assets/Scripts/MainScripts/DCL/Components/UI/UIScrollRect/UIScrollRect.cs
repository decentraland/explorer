using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace DCL.Components
{
    public class UIScrollRect : UIShape<UIScrollRectRefContainer, UIScrollRect.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public float valueX = 0;
            public float valueY = 0;
            public Color borderColor = Color.white;
            public Color backgroundColor = Color.clear;
            public bool isHorizontal = false;
            public bool isVertical = true;
            public float paddingTop = 0f;
            public float paddingRight = 0f;
            public float paddingBottom = 0f;
            public float paddingLeft = 0f;
            public string OnChanged;

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
                       valueX == model.valueX &&
                       valueY == model.valueY &&
                       borderColor.Equals(model.borderColor) &&
                       backgroundColor.Equals(model.backgroundColor) &&
                       isHorizontal == model.isHorizontal &&
                       isVertical == model.isVertical &&
                       paddingTop == model.paddingTop &&
                       paddingRight == model.paddingRight &&
                       paddingBottom == model.paddingBottom &&
                       paddingLeft == model.paddingLeft &&
                       OnChanged == model.OnChanged;
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = -667376577;
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
                hashCode = hashCode * -1521134295 + valueX.GetHashCode();
                hashCode = hashCode * -1521134295 + valueY.GetHashCode();
                hashCode = hashCode * -1521134295 + borderColor.GetHashCode();
                hashCode = hashCode * -1521134295 + backgroundColor.GetHashCode();
                hashCode = hashCode * -1521134295 + isHorizontal.GetHashCode();
                hashCode = hashCode * -1521134295 + isVertical.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingTop.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingRight.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingBottom.GetHashCode();
                hashCode = hashCode * -1521134295 + paddingLeft.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(OnChanged);
                return hashCode;
            }
        }

        public override string referencesContainerPrefabName => "UIScrollRect";

        public UIScrollRect(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScrollRectShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override void OnChildAttached(UIShape parent, UIShape childComponent)
        {
            base.OnChildAttached(parent, childComponent);
            childComponent.OnAppliedChanges -= RefreshContainerForShape;
            childComponent.OnAppliedChanges += RefreshContainerForShape;
            RefreshContainerForShape(childComponent);
        }

        public override void OnChildDetached(UIShape parent, UIShape childComponent)
        {
            base.OnChildDetached(parent, childComponent);
            childComponent.OnAppliedChanges -= RefreshContainerForShape;
        }

        void RefreshContainerForShape(BaseDisposable updatedComponent)
        {
            RefreshAll();
            referencesContainer.fitter.RefreshRecursively();
            AdjustChildHook();
            referencesContainer.scrollRect.Rebuild(CanvasUpdate.MaxUpdateValue);
        }

        void AdjustChildHook()
        {
            UIScrollRectRefContainer rc = referencesContainer;
            rc.childHookRectTransform.SetParent(rc.layoutElementRT, false);
            rc.childHookRectTransform.SetToMaxStretch();
            rc.childHookRectTransform.SetParent(rc.content, true);
            RefreshDCLLayoutRecursively(false, true);
        }

        public override void RefreshDCLLayoutRecursively(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            base.RefreshDCLLayoutRecursively(refreshSize, refreshAlignmentAndPosition);
            referencesContainer.fitter.RefreshRecursively();
        }


        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            UIScrollRectRefContainer rc = referencesContainer;

            rc.contentBackground.color = model.backgroundColor;

            // Apply padding
            rc.paddingLayoutGroup.padding.bottom = Mathf.RoundToInt(model.paddingBottom);
            rc.paddingLayoutGroup.padding.top = Mathf.RoundToInt(model.paddingTop);
            rc.paddingLayoutGroup.padding.left = Mathf.RoundToInt(model.paddingLeft);
            rc.paddingLayoutGroup.padding.right = Mathf.RoundToInt(model.paddingRight);

            rc.scrollRect.horizontal = model.isHorizontal;
            rc.scrollRect.vertical = model.isVertical;

            rc.HScrollbar.value = model.valueX;
            rc.VScrollbar.value = model.valueY;

            rc.scrollRect.onValueChanged.AddListener(OnChanged);

            RefreshAll();
            referencesContainer.fitter.RefreshRecursively();
            AdjustChildHook();
            return null;
        }

        void OnChanged(Vector2 scrollingValues)
        {
            WebInterface.ReportOnScrollChange(scene.sceneData.id, model.OnChanged, scrollingValues, 0);
        }

        public override void Dispose()
        {
            if (referencesContainer != null)
            {
                referencesContainer.scrollRect.onValueChanged.RemoveAllListeners();
                Utils.SafeDestroy(referencesContainer.gameObject);
            }

            base.Dispose();
        }
    }
}