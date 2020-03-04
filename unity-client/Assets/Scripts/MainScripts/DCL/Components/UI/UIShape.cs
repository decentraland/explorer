using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace DCL.Components
{
    [System.Serializable]
    public struct UIValue
    {
        public enum Unit
        {
            PERCENT,
            PIXELS
        }

        public float value;
        public Unit type;

        public void SetPixels(float value)
        {
            this.type = Unit.PIXELS;
            this.value = value;
        }

        public void SetPercent(float value)
        {
            this.type = Unit.PERCENT;
            this.value = value;
        }

        public UIValue(float value, Unit unitType = Unit.PIXELS)
        {
            this.value = value;
            this.type = unitType;
        }

        public float GetScaledValue(float parentSize)
        {
            if (type == Unit.PIXELS) return value;

            return value / 100 * parentSize;
        }
    }

    public class UIShape<ReferencesContainerType, ModelType> : UIShape
        where ReferencesContainerType : UIReferencesContainer
        where ModelType : UIShape.Model
    {
        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        new public ModelType model
        {
            get { return base.model as ModelType; }
            set { base.model = value; }
        }

        new public ReferencesContainerType referencesContainer
        {
            get { return base.referencesContainer as ReferencesContainerType; }
            set { base.referencesContainer = value; }
        }

        public override ComponentUpdateHandler CreateUpdateHandler()
        {
            return new UIShapeUpdateHandler<ReferencesContainerType, ModelType>(this);
        }

        bool raiseOnAttached;
        bool firstApplyChangesCall;

        /// <summary>
        /// This is called by UIShapeUpdateHandler before calling ApplyChanges.
        /// </summary>
        public void PreApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<ModelType>(newJson);

            raiseOnAttached = false;
            firstApplyChangesCall = false;

            if (referencesContainer == null)
            {
                referencesContainer = InstantiateUIGameObject<ReferencesContainerType>(referencesContainerPrefabName);

                raiseOnAttached = true;
                firstApplyChangesCall = true;
            }
            else if (ReparentComponent(referencesContainer.rectTransform, model.parentComponent))
            {
                raiseOnAttached = true;
            }
        }

        public override void RaiseOnAppliedChanges()
        {
            RefreshDCLLayout();

#if UNITY_EDITOR
            SetComponentDebugName();
#endif

            // We hide the component visibility when it's created (first applychanges)
            // as it has default values and appears in the middle of the screen
            if (firstApplyChangesCall)
                referencesContainer.canvasGroup.alpha = 0f;
            else
                referencesContainer.canvasGroup.alpha = model.visible ? model.opacity : 0f;

            referencesContainer.canvasGroup.blocksRaycasts = model.visible && model.isPointerBlocker;

            base.RaiseOnAppliedChanges();

            if (raiseOnAttached && parentUIComponent != null)
            {
                UIReferencesContainer[] parents = referencesContainer.GetComponentsInParent<UIReferencesContainer>(true);

                for (int i = 0; i < parents.Length; i++)
                {
                    UIReferencesContainer parent = parents[i];
                    if (parent.owner != null)
                    {
                        parent.owner.OnChildAttached(parentUIComponent, this);
                    }
                }
            }
        }
    }

    public class UIShape : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string name;
            public string parentComponent;
            public bool visible = true;
            public float opacity = 1f;
            public string hAlign = "center";
            public string vAlign = "center";
            public UIValue width = new UIValue(100f);
            public UIValue height = new UIValue(50f);
            public UIValue positionX = new UIValue(0f);
            public UIValue positionY = new UIValue(0f);
            public bool isPointerBlocker = true;
            public string onClick;
        }

        public override string componentName => GetDebugName();
        public virtual string referencesContainerPrefabName => "";
        public UIReferencesContainer referencesContainer;
        public RectTransform childHookRectTransform;

        public LinkedList<UIShape> uiTree;
        public LinkedListNode<UIShape> uiTreeNode;

        public Model model = new Model();

        public UIShape parentUIComponent { get; private set; }

        public UIShape(ParcelScene scene) : base(scene)
        {
        }

        public string GetDebugName()
        {
            if (string.IsNullOrEmpty(model.name))
            {
                return GetType().Name;
            }
            else
            {
                return GetType().Name + " - " + model.name;
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            return null;
        }

        internal T InstantiateUIGameObject<T>(string prefabPath) where T : UIReferencesContainer
        {
            GameObject uiGameObject = null;
            bool targetParentExists = !string.IsNullOrEmpty(model.parentComponent) &&
                                      scene.disposableComponents.ContainsKey(model.parentComponent);

            if (targetParentExists)
            {
                if (scene.disposableComponents.ContainsKey(model.parentComponent))
                    parentUIComponent = (scene.disposableComponents[model.parentComponent] as UIShape);
                else
                    parentUIComponent = scene.uiScreenSpace as UIShape;
            }
            else
            {
                parentUIComponent = scene.uiScreenSpace as UIShape;
            }

            uiGameObject = UnityEngine.Object.Instantiate(Resources.Load(prefabPath), parentUIComponent.childHookRectTransform) as GameObject;

            referencesContainer = uiGameObject.GetComponent<T>();
            referencesContainer.rectTransform.SetToMaxStretch();

            childHookRectTransform = referencesContainer.childHookRectTransform;

            referencesContainer.owner = this;

            ConfigureUITreeNode();

            return referencesContainer as T;
        }

        protected void ConfigureUITreeNode()
        {
            if (this is UIScreenSpace)
            {
                Debug.Log("CREATING NEW TREE");

                uiTree = new LinkedList<UIShape>();
                uiTreeNode = uiTree.AddLast(this);
            }
            else if (parentUIComponent != null && parentUIComponent.uiTree != null)
            {
                Debug.Log(referencesContainer.gameObject.name + " USING PARENT TREE FROM " + parentUIComponent.childHookRectTransform.parent.name, childHookRectTransform.parent);

                if (uiTree != null)
                {
                    Debug.Log("RELEASING PREVIOUS TREE");
                    uiTree.Remove(uiTreeNode);
                }

                uiTree = parentUIComponent.uiTree;
                uiTreeNode = uiTree.AddAfter(parentUIComponent.uiTreeNode, this);
            }
            else
            {
                Debug.Log("DIDN'T SET TREE: " + parentUIComponent.uiTree);
            }
        }

        public virtual void RefreshAll()
        {
            Debug.Log("REFRESH ALL");

            /* RefreshDCLLayoutRecursively();
            FixMaxStretchRecursively();
            RefreshDCLLayoutRecursively_Internal(refreshSize: false, refreshAlignmentAndPosition: true); */

            if (uiTree == null)
            {
                Debug.Log("uiTree is null", referencesContainer.transform);
                return;
            }

            // 1 - Fully RefreshDCLLayout
            TraverseUITree((referencesContainer) => referencesContainer.owner.RefreshDCLLayout(true, true));

            // 2 - Fix max stretch
            TraverseUITree((referencesContainer) => referencesContainer.rectTransform.SetToMaxStretch());

            // 3 - RefreshDCLLayout (Alignment and position only)
            TraverseUITree((referencesContainer) => referencesContainer.owner.RefreshDCLLayout(false, true));



            // Debug.Log("Inversed Tree:");
            TraverseUITree((referencesContainer) => Debug.Log(referencesContainer.childHookRectTransform.parent.name, referencesContainer.childHookRectTransform.parent));
        }

        void TraverseUITree(System.Action<UIReferencesContainer> callback)
        {
            int uiTreeCount = uiTree.Count;
            bool finishedTraversing = uiTreeCount == 0;
            var currentNode = uiTree.Last;

            while (!finishedTraversing)
            {
                if (currentNode == null || currentNode.Value.referencesContainer == null)
                {
                    if (currentNode.Previous != null)
                        currentNode = currentNode.Previous;
                    else
                        finishedTraversing = true;

                    continue;
                }

                if (currentNode.Value.referencesContainer.owner != null)
                {
                    callback(currentNode.Value.referencesContainer);
                }

                if (currentNode.Previous != null)
                    currentNode = currentNode.Previous;
                else
                    finishedTraversing = true;
            }
        }

        public virtual void RefreshDCLLayout(bool refreshSize = true, bool refreshAlignmentAndPosition = true)
        {
            RectTransform parentRT = referencesContainer.GetComponentInParent<RectTransform>();

            if (refreshSize)
            {
                RefreshDCLSize(parentRT);
            }

            if (refreshAlignmentAndPosition)
            {
                // Alignment (Alignment uses size so we should always align AFTER resizing)
                RefreshDCLAlignmentAndPosition(parentRT);
            }
        }

        protected void RefreshDCLSize(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
            {
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();
            }

            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                model.width.GetScaledValue(parentTransform.rect.width));
            referencesContainer.layoutElementRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                model.height.GetScaledValue(parentTransform.rect.height));

            referencesContainer.layoutElementRT.ForceUpdateRectTransforms();
        }

        public void RefreshDCLAlignmentAndPosition(RectTransform parentTransform = null)
        {
            if (parentTransform == null)
            {
                parentTransform = referencesContainer.GetComponentInParent<RectTransform>();
            }

            referencesContainer.layoutElement.ignoreLayout = false;
            ConfigureAlignment(referencesContainer.layoutGroup);
            LayoutRebuilder.ForceRebuildLayoutImmediate(referencesContainer.layoutGroup.transform as RectTransform);
            referencesContainer.layoutElement.ignoreLayout = true;

            // Reposition
            Vector3 position = Vector3.zero;
            position.x = model.positionX.GetScaledValue(parentTransform.rect.width);
            position.y = model.positionY.GetScaledValue(parentTransform.rect.height);

            referencesContainer.layoutElementRT.localPosition += position;
        }

        public virtual void RefreshDCLLayoutRecursively(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            RefreshDCLLayoutRecursively_Internal(refreshSize, refreshAlignmentAndPosition);
        }

        public void RefreshDCLLayoutRecursively_Internal(bool refreshSize = true,
            bool refreshAlignmentAndPosition = true)
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            Utils.InverseTransformChildTraversal<UIReferencesContainer>(
                (x) =>
                {
                    if (x.owner != null)
                    {
                        x.owner.RefreshDCLLayout(refreshSize, refreshAlignmentAndPosition);
                    }
                },
                rootParent.referencesContainer.transform);
        }

        public void FixMaxStretchRecursively()
        {
            UIShape rootParent = GetRootParent();

            Assert.IsTrue(rootParent != null, "root parent must never be null");

            Utils.InverseTransformChildTraversal<UIReferencesContainer>(
                (x) =>
                {
                    if (x.owner != null)
                    {
                        x.rectTransform.SetToMaxStretch();
                    }
                },
                rootParent.referencesContainer.transform);
        }

        protected bool ReparentComponent(RectTransform targetTransform, string targetParent)
        {
            Debug.Log("REPARENT COMPONENT");

            bool targetParentExists = !string.IsNullOrEmpty(targetParent) &&
                                      scene.disposableComponents.ContainsKey(targetParent);

            if (targetParentExists && parentUIComponent == scene.disposableComponents[targetParent])
            {
                // ConfigureUITreeNode();
                return false;
            }

            Debug.Log("REPARENT COMPONENT - 2");

            if (parentUIComponent != null)
            {
                UIReferencesContainer[] parents = referencesContainer.GetComponentsInParent<UIReferencesContainer>(true);

                foreach (var parent in parents)
                {
                    if (parent.owner != null)
                    {
                        parent.owner.OnChildDetached(parentUIComponent, this);
                    }
                }
            }

            if (targetParentExists)
            {
                parentUIComponent = scene.disposableComponents[targetParent] as UIShape;
            }
            else
            {
                parentUIComponent = scene.uiScreenSpace as UIShape;
            }

            targetTransform.SetParent(parentUIComponent.childHookRectTransform, false);

            ConfigureUITreeNode();

            Debug.Log("REPARENT COMPONENT - 3");

            return true;
        }

        public UIShape GetRootParent()
        {
            UIShape parent = null;

            if (parentUIComponent != null && !(parentUIComponent is UIScreenSpace))
            {
                parent = parentUIComponent.GetRootParent();
            }
            else
            {
                parent = this;
            }

            return parent;
        }

        protected void ConfigureAlignment(LayoutGroup layout)
        {
            switch (model.vAlign)
            {
                case "top":
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.UpperLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.UpperRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.UpperCenter;
                            break;
                    }

                    break;
                case "bottom":
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.LowerLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.LowerRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.LowerCenter;
                            break;
                    }

                    break;
                default: // center
                    switch (model.hAlign)
                    {
                        case "left":
                            layout.childAlignment = TextAnchor.MiddleLeft;
                            break;
                        case "right":
                            layout.childAlignment = TextAnchor.MiddleRight;
                            break;
                        default:
                            layout.childAlignment = TextAnchor.MiddleCenter;
                            break;
                    }

                    break;
            }
        }

        protected void SetComponentDebugName()
        {
            if (referencesContainer == null || model == null)
            {
                return;
            }

            referencesContainer.name = componentName;
        }

        public override void Dispose()
        {
            uiTree.Remove(uiTreeNode);

            if (childHookRectTransform)
                Utils.SafeDestroy(childHookRectTransform.gameObject);

            base.Dispose();
        }

        public virtual void OnChildAttached(UIShape parentComponent, UIShape childComponent) { }
        public virtual void OnChildDetached(UIShape parentComponent, UIShape childComponent) { }
    }
}
