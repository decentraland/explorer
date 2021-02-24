using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIContainerStack : UIShape<UIContainerRectReferencesContainer, UIContainerStack.Model>
    {
        [System.Serializable]
        new public class Model : UIShape.Model
        {
            public Color color = Color.clear;
            public StackOrientation stackOrientation = StackOrientation.VERTICAL;
            public bool adaptWidth = true;
            public bool adaptHeight = true;
            public float spacing = 0;

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
                       color.Equals(model.color) &&
                       stackOrientation == model.stackOrientation &&
                       adaptWidth == model.adaptWidth &&
                       adaptHeight == model.adaptHeight &&
                       spacing == model.spacing;
            }

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override int GetHashCode()
            {
                int hashCode = 139398335;
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
                hashCode = hashCode * -1521134295 + color.GetHashCode();
                hashCode = hashCode * -1521134295 + stackOrientation.GetHashCode();
                hashCode = hashCode * -1521134295 + adaptWidth.GetHashCode();
                hashCode = hashCode * -1521134295 + adaptHeight.GetHashCode();
                hashCode = hashCode * -1521134295 + spacing.GetHashCode();
                return hashCode;
            }
        }

        public enum StackOrientation
        {
            VERTICAL,
            HORIZONTAL
        }

        public override string referencesContainerPrefabName => "UIContainerRect";

        public Dictionary<string, GameObject> stackContainers = new Dictionary<string, GameObject>();

        HorizontalOrVerticalLayoutGroup layoutGroup;

        public UIContainerStack(ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int)CLASS_ID.UI_CONTAINER_STACK;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIContainerStack attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            referencesContainer.image.color = new Color(model.color.r, model.color.g, model.color.b, model.color.a);

            if (model.stackOrientation == StackOrientation.VERTICAL && !(layoutGroup is VerticalLayoutGroup))
            {
                Object.DestroyImmediate(layoutGroup, false);
                layoutGroup = childHookRectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            else if (model.stackOrientation == StackOrientation.HORIZONTAL && !(layoutGroup is HorizontalLayoutGroup))
            {
                Object.DestroyImmediate(layoutGroup, false);
                layoutGroup = childHookRectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = model.spacing;

            referencesContainer.sizeFitter.adjustHeight = model.adaptHeight;
            referencesContainer.sizeFitter.adjustWidth = model.adaptWidth;

            RefreshAll();
            return null;
        }

        void RefreshContainerForShape(BaseDisposable updatedComponent)
        {
            UIShape childComponent = updatedComponent as UIShape;
            Assert.IsTrue(childComponent != null, "This should never happen!!!!");
        
            if (((UIShape.Model)childComponent.GetModel()).parentComponent != id)
            {
                RefreshAll();
                return;
            }

            GameObject stackContainer = null;

            if (!stackContainers.ContainsKey(childComponent.id))
            {
                stackContainer = Object.Instantiate(Resources.Load("UIContainerStackChild")) as GameObject;
#if UNITY_EDITOR
                stackContainer.name = "UIContainerStackChild - " + childComponent.id;
#endif
                stackContainers.Add(childComponent.id, stackContainer);

                int oldSiblingIndex = childComponent.referencesContainer.transform.GetSiblingIndex();
                childComponent.referencesContainer.transform.SetParent(stackContainer.transform, false);
                stackContainer.transform.SetParent(referencesContainer.childHookRectTransform, false);
                stackContainer.transform.SetSiblingIndex(oldSiblingIndex);
            }
            else
            {
                stackContainer = stackContainers[childComponent.id];
            }

            RefreshAll();
        }

        public override void OnChildAttached(UIShape parentComponent, UIShape childComponent)
        {
            RefreshContainerForShape(childComponent);
            childComponent.OnAppliedChanges -= RefreshContainerForShape;
            childComponent.OnAppliedChanges += RefreshContainerForShape;
        }

        public override void RefreshDCLLayoutRecursively(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            base.RefreshDCLLayoutRecursively(refreshSize, refreshAlignmentAndPosition);
            referencesContainer.sizeFitter.RefreshRecursively();
        }

        public override void OnChildDetached(UIShape parentComponent, UIShape childComponent)
        {
            if (parentComponent != this)
            {
                return;
            }

            if (stackContainers.ContainsKey(childComponent.id))
            {
                Object.Destroy(stackContainers[childComponent.id]);
                stackContainers[childComponent.id].transform.SetParent(null);
                stackContainers[childComponent.id].name += "- Detached";
                stackContainers.Remove(childComponent.id);
            }

            childComponent.OnAppliedChanges -= RefreshContainerForShape;
            RefreshDCLLayout();
        }

        public override void Dispose()
        {
            if (referencesContainer != null)
                Utils.SafeDestroy(referencesContainer.gameObject);

            base.Dispose();
        }
    }
}