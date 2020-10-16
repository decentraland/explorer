using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Components
{
    public class UIScreenSpace : UIShape
    {
        static bool VERBOSE = false;

        public Canvas canvas;

        private DCLCharacterPosition currentCharacterPosition;
        private CanvasGroup canvasGroup;

        public UIScreenSpace(ParcelScene scene) : base(scene)
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;

            //Only no-dcl scenes are listening the the global visibility event
            if (!scene.isPersistent)
            {
                CommonScriptableObjects.allUIHidden.OnChange += AllUIHidden_OnChange;
            }
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            Debug.LogError(
                "Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (scene.uiScreenSpace == null)
            {
                scene.uiScreenSpace = this;

                InitializeCanvas();
            }
            else if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.characterPosition);
            }

            //We have to wait a frame for the Canvas Scaler to act
            yield return null;
        }

        public override void Dispose()
        {
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;
            CommonScriptableObjects.allUIHidden.OnChange -= AllUIHidden_OnChange;

            if (childHookRectTransform != null)
            {
                Utils.SafeDestroy(childHookRectTransform.gameObject);
            }
        }

        void OnCharacterMoved(DCLCharacterPosition newCharacterPosition)
        {
            if (canvas != null)
            {
                currentCharacterPosition = newCharacterPosition;

                UpdateCanvasVisibility();

                if (VERBOSE)
                {
                    Debug.Log($"set screenspace = {currentCharacterPosition}");
                }
            }
        }

        private void AllUIHidden_OnChange(bool current, bool previous)
        {
            UpdateCanvasVisibility();
        }

        private void UpdateCanvasVisibility()
        {
            if (canvas != null && scene != null)
            {
                bool isInsideSceneBounds = scene.IsInsideSceneBoundaries(Utils.WorldToGridPosition(currentCharacterPosition.worldPosition));
                bool shouldBeVisible = scene.isPersistent || (model.visible && isInsideSceneBounds && !CommonScriptableObjects.allUIHidden.Get());
                canvasGroup.alpha = shouldBeVisible ? 1f : 0f;
                canvasGroup.blocksRaycasts = shouldBeVisible;
            }
        }

        void InitializeCanvas()
        {
            if (VERBOSE)
            {
                Debug.Log("Started canvas initialization in " + id);
            }

            GameObject canvasGameObject = new GameObject("UIScreenSpace");
            canvasGameObject.layer = LayerMask.NameToLayer("UI");
            canvasGameObject.transform.SetParent(scene.transform);
            canvasGameObject.transform.ResetLocalTRS();

            // Canvas
            canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Canvas Scaler (for maintaining ui aspect ratio)
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            canvasScaler.matchWidthOrHeight = 1f; // Match height, recommended for landscape projects

            // Graphics Raycaster (for allowing touch/click input on the ui components)
            canvasGameObject.AddComponent<GraphicRaycaster>();

            if (scene.isPersistent)
            {
                childHookRectTransform = canvas.GetComponent<RectTransform>();

                // we make sure DCL UI renders above every parcel UI
                canvas.sortingOrder = 1;
            }
            else
            {
                canvas.sortingOrder = -1;

                GameObject resizedPanel = new GameObject("ResizeUIArea");

                resizedPanel.AddComponent<RectMask2D>().enabled = false; // TODO: find a way of avoiding this RectMask (right now if we remove it that gameobject gets destroyed somehow...)

                resizedPanel.AddComponent<CanvasRenderer>();
                childHookRectTransform = resizedPanel.GetComponent<RectTransform>();
                childHookRectTransform.SetParent(canvas.transform);
                childHookRectTransform.ResetLocalTRS();

                childHookRectTransform.anchorMin = Vector2.zero;
                childHookRectTransform.anchorMax = new Vector2(1, 0);

                float canvasHeight = canvasScaler.referenceResolution.y;
                childHookRectTransform.pivot = new Vector2(0.5f, 1f);
                childHookRectTransform.anchoredPosition = new Vector3(0, canvasHeight, 0f);
                float canvasSubtraction = canvasHeight * 0.05f;
                childHookRectTransform.sizeDelta = new Vector2(0, canvasHeight - canvasSubtraction);
            }

            // Canvas group
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Alpha will be updated later when the player enters this scene
            canvasGroup.blocksRaycasts = false;

            if (VERBOSE)
            {
                Debug.Log("canvas initialized, width: " + childHookRectTransform.rect.width);
                Debug.Log("canvas initialized, height: " + childHookRectTransform.rect.height);
            }

            if (DCLCharacterController.i != null)
            {
                OnCharacterMoved(DCLCharacterController.i.characterPosition);
            }

            if (VERBOSE)
            {
                Debug.Log("Finished canvas initialization in " + id);
            }

            if (!scene.isPersistent)
            {
                UpdateCanvasVisibility();
            }
        }
    }
}
