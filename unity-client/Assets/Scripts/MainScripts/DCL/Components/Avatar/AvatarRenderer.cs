using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarRenderer : MonoBehaviour
    {
        public Material defaultMaterial;
        public Material eyeMaterial;
        public Material eyebrowMaterial;
        public Material mouthMaterial;
        public AnimationClip[] maleAnims;
        public AnimationClip[] femaleAnims;

        AvatarModel model;

        Action OnSuccessCallback;
        Action OnFailCallback;

        private bool visibility = true;
        
        protected BodyShapeController bodyShapeController;
        protected Dictionary<string, WearableController> wearablesController = new Dictionary<string, WearableController>();
        FacialFeatureController eyesController;
        FacialFeatureController eyebrowsController;
        FacialFeatureController mouthController;

        public void ApplyModel (AvatarModel model, Action onSuccess, Action onFail)
        {
            this.model = model;
            this.OnSuccessCallback = onSuccess;
            this.OnFailCallback = onFail;

            StopAllCoroutines();
            if (this.model == null)
            {
                ResetAvatar();
                this.OnSuccessCallback?.Invoke();
                return;
            }

            StartCoroutine(LoadAvatar());
        }

        public void ResetAvatar()
        {
            bodyShapeController?.CleanUp();
            bodyShapeController = null;
            using (var iterator = wearablesController.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.CleanUp();
                }
            }

            wearablesController.Clear();
        }

        void CleanUpUnusedItems()
        {
            if (model.wearables != null)
            {
                var ids = wearablesController.Keys.ToArray();
                for (var i = 0; i < ids.Length; i++)
                {
                    var currentId = ids[i];
                    var wearable = wearablesController[currentId];
                    if (!wearable.isReady || !model.wearables.Contains(wearable.id))
                    {
                        wearable.CleanUp();
                        wearablesController.Remove(currentId);
                    }
                }
            }
        }

        private IEnumerator LoadAvatar ()
        {
            if (string.IsNullOrEmpty(model.bodyShape))
            {
                this.OnSuccessCallback?.Invoke();
                yield break;
            }

            if (bodyShapeController != null && bodyShapeController.id != model?.bodyShape)
            {
                bodyShapeController?.CleanUp();
                bodyShapeController = null;
            }

            if (bodyShapeController == null)
            {
                HideAll();

                bodyShapeController = new BodyShapeController( ResolveWearable(model.bodyShape));
                SetupDefaultFacialFeatures(bodyShapeController.bodyShapeType);
                bodyShapeController.Load(transform, visibility, OnWearableLoadingSuccess, OnWearableLoadingFail);
            }
            else
            {
                //If bodyShape is downloading will call OnWearableLoadingSuccess (and therefore SetupDefaultMaterial) once ready
                if (bodyShapeController.isReady)
                    bodyShapeController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
            }

            int wearableCount = model.wearables.Count;
            for (int index = 0; index < wearableCount; index++)
            {
                var wearableId = this.model.wearables[index];

                if (!wearablesController.ContainsKey(wearableId))
                {
                    ProcessWearable(wearableId);
                }
                else
                {
                    UpdateWearable(wearableId);
                }
            }

            yield return new WaitUntil(AreDownloadsReady);

            bodyShapeController.RemoveUnusedParts();

            bool eyesReady = false;
            bool eyebrowsReady = false;
            bool mouthReady = false;

            StartCoroutine(eyesController?.FetchTextures((mainTexture, maskTexture) =>
            {
                eyesReady = true;
                bodyShapeController.SetupEyes(eyeMaterial, mainTexture, maskTexture, model.eyeColor);
            }));
            StartCoroutine(eyebrowsController?.FetchTextures((mainTexture, maskTexture) =>
            {
                eyebrowsReady = true;
                bodyShapeController.SetupEyebrows(eyebrowMaterial, mainTexture, model.hairColor);
            }));

            StartCoroutine(mouthController?.FetchTextures((mainTexture, maskTexture) =>
            {
                mouthReady = true;
                bodyShapeController.SetupMouth(mouthMaterial, mainTexture, model.skinColor);
            }));

            yield return new WaitUntil( () => eyesReady && eyebrowsReady && mouthReady);

            SetupAnimator();

            yield return null;

            CleanUpUnusedItems();

            yield return null;
            ResolveVisibility();

            OnSuccessCallback?.Invoke();
        }

        void OnWearableLoadingSuccess(WearableController wearableController)
        {
            wearableController.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
        }

        void OnWearableLoadingFail(WearableController wearableController)
        {
            Debug.LogError($"Avatar: {model.name}  -  Failed loading wearable: {wearableController.id}");
            StopAllCoroutines();

            ResetAvatar();
            OnFailCallback?.Invoke();
        }

        void SetupAnimator()
        {
            AvatarAnimatorLegacy animator = GetComponent<AvatarAnimatorLegacy>();
            Animation animation = bodyShapeController.PrepareAnimation();
            string bodyShapeType = bodyShapeController.bodyShapeType;

            AnimationClip[] animArray = null;

            if (bodyShapeType.Contains(WearableLiterals.BodyShapes.MALE))
            {
                animArray = maleAnims;
            }
            else if (bodyShapeType.Contains(WearableLiterals.BodyShapes.FEMALE))
            {
                animArray = femaleAnims;
            }

            for (int index = 0; index < animArray.Length; index++)
            {
                var clip = animArray[index];
                if (animation.GetClip(clip.name) == null)
                    animation.AddClip(clip, clip.name);
            }

            animator.target = transform;
            animator.animation = animation;

            //NOTE(Brian): Set bones/rootBone of all wearables to be the same of the baseBody,
            //             so all of them are animated together.
            var mainSkinnedRenderer = bodyShapeController.GetSkinnedMeshRenderer();
            using (var enumerator = wearablesController.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Value.SetAnimatorBones(mainSkinnedRenderer);
                }
            }

            animator.SetIdleFrame();
        }


        bool AreDownloadsReady()
        {
            if (!bodyShapeController.isReady)
                return false;

            using (var iterator = wearablesController.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var wearable = iterator.Current.Value;
                    if (!wearable.isReady)
                        return false;
                }
            }

            return true;
        }

        private void ResolveVisibility()
        {
            if (bodyShapeController == null) return;
            
            HashSet<string> hiddenCategories = CreateHiddenList();

            if(bodyShapeController.isReady)
                bodyShapeController.SetAssetRenderersEnabled(visibility && !hiddenCategories.Contains(WearableLiterals.Misc.HEAD));

            using (var iterator = wearablesController.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var wearableController = iterator.Current.Value;
                    if (wearableController.isReady)
                    {
                        var wearable = wearableController.wearable;
                        wearableController.SetAssetRenderersEnabled(visibility && !hiddenCategories.Contains(wearable.category));
                    }
                }
            }
        }

        private HashSet<string> CreateHiddenList()
        {
            HashSet<string> hiddenCategories = new HashSet<string>();
            if (model?.wearables != null)
            {
                //Last wearable added has priority over the rest
                for (int i = model.wearables.Count - 1; i >= 0; i--)
                {
                    string id = model.wearables[i];
                    if (!wearablesController.ContainsKey(id)) continue;

                    var wearable = wearablesController[id].wearable;

                    if (hiddenCategories.Contains(wearable.category)) //Skip hidden elements to avoid two elements hiding each other
                        continue;

                    var wearableHidesList = wearable.GetHidesList(bodyShapeController.bodyShapeType);
                    if (wearableHidesList != null)
                    {
                        hiddenCategories.UnionWith(wearableHidesList);
                    }
                }
            }

            return hiddenCategories;
        }

        public void SetVisibility(bool newVisibility)
        {
            if (visibility == newVisibility) return;
            visibility = newVisibility;
            ResolveVisibility();
        }

        private void HideAll()
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }

        protected virtual void OnDestroy()
        {
            ResetAvatar();
        }

        private void ProcessWearable(string wearableId)
        {
            var wearable = ResolveWearable(wearableId);
            if (wearable == null) return;

            switch (wearable.category)
            {
                case WearableLiterals.Categories.EYES:
                    eyesController = new FacialFeatureController(wearable, bodyShapeController.bodyShapeType);
                    break;
                case WearableLiterals.Categories.EYEBROWS:
                    eyebrowsController = new FacialFeatureController(wearable, bodyShapeController.bodyShapeType);
                    break;
                case WearableLiterals.Categories.MOUTH:
                    mouthController = new FacialFeatureController(wearable, bodyShapeController.bodyShapeType);
                    break;
                case WearableLiterals.Categories.BODY_SHAPE:
                    break;

                default:
                    var wearableController = new WearableController(ResolveWearable(wearableId), bodyShapeController.id);
                    wearablesController.Add(wearableId, wearableController);
                    wearableController.Load(transform, visibility, OnWearableLoadingSuccess, OnWearableLoadingFail);
                    break;
            }
        }

        private void UpdateWearable(string wearableId)
        {
            var wearable = wearablesController[wearableId];
            switch (wearable.category)
            {
                case WearableLiterals.Categories.EYES:
                case WearableLiterals.Categories.EYEBROWS:
                case WearableLiterals.Categories.MOUTH:
                case WearableLiterals.Categories.BODY_SHAPE:
                    break;
                default:
                    //If wearable is downloading will call OnWearableLoadingSuccess(and therefore SetupDefaultMaterial) once ready
                    if(wearable.isReady)
                        wearable.SetupDefaultMaterial(defaultMaterial, model.skinColor, model.hairColor);
                    break;
            }
        }

        WearableItem ResolveWearable(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (!CatalogController.wearableCatalog.TryGetValue(id, out WearableItem wearable))
            {
                Debug.LogError($"Wearable {id} not found in catalog");
            }

            return wearable;
        }

        private void SetupDefaultFacialFeatures(string bodyShape)
        {
            string eyesDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.EYES);
            eyesController = new FacialFeatureController(ResolveWearable(eyesDefaultId), bodyShapeController.bodyShapeType);

            string eyebrowsDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.EYEBROWS);
            eyebrowsController = new FacialFeatureController(ResolveWearable(eyebrowsDefaultId), bodyShapeController.bodyShapeType);

            string mouthDefaultId = WearableLiterals.DefaultWearables.GetDefaultWearable(bodyShape, WearableLiterals.Categories.MOUTH);
            mouthController = new FacialFeatureController(ResolveWearable(mouthDefaultId), bodyShapeController.bodyShapeType);
        }

        protected void CopyFrom(AvatarRenderer original)
        {
            this.wearablesController = original.wearablesController;
            this.mouthController = original.mouthController;
            this.bodyShapeController = original.bodyShapeController;
            this.eyebrowsController = original.eyebrowsController;
            this.eyesController = original.eyesController;
        }
    }
}