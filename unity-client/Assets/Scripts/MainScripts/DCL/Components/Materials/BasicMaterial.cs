using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.Components
{
    public class BasicMaterial : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string texture;

            // value that defines if a pixel is visible or invisible (no transparency gradients)
            [Range(0f, 1f)] public float alphaTest = 0.5f;
            public bool castShadows = true;
        }

        public Model model = new Model();
        public Material material;

        private DCLTexture dclTexture = null;

        private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int _AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int _Cutoff = Shader.PropertyToID("_Cutoff");
        private static readonly int _ZWrite = Shader.PropertyToID("_ZWrite");

        public BasicMaterial(ParcelScene scene) : base(scene)
        {
            material = new Material(Utils.EnsureResourcesMaterial("Materials/BasicShapeMaterial"));

            OnAttach += OnMaterialAttached;
            OnDetach += OnMaterialDetached;
        }

        public override int GetClassId()
        {
            return (int)CLASS_ID.BASIC_MATERIAL;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
                return;

            entity.RemoveSharedComponent(typeof(PBRMaterial));
            base.AttachTo(entity);
        }

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (material == null)
            {
                yield break; // We escape ApplyChanges called in the parent's constructor
            }

#if UNITY_EDITOR
            material.name = "BasicMaterial_" + id;
#endif

            model = Utils.SafeFromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.texture))
            {
                if (dclTexture == null || dclTexture.id != model.texture)
                {
                    yield return DCLTexture.FetchTextureComponent(scene, model.texture, (downloadedTexture) =>
                    {
                        dclTexture?.DetachFrom(this);
                        material.SetTexture(_BaseMap, downloadedTexture.texture);
                        dclTexture = downloadedTexture;
                        dclTexture.AttachTo(this);
                    });
                }
            }
            else
            {
                material.mainTexture = null;
                dclTexture?.DetachFrom(this);
                dclTexture = null;
            }

            material.EnableKeyword("_ALPHATEST_ON");
            material.SetInt(_ZWrite, 1);
            material.SetFloat(_AlphaClip, 1);
            material.SetFloat(_Cutoff, model.alphaTest);
            material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
            foreach (DecentralandEntity decentralandEntity in attachedEntities)
            {
                InitMaterial(decentralandEntity.meshRootGameObject);
            }
        }

        void OnMaterialAttached(DecentralandEntity entity)
        {
            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            if (entity.meshRootGameObject != null)
            {
                var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                    InitMaterial(entity.meshRootGameObject);
            }
        }

        void InitMaterial(GameObject meshGameObject)
        {
            if (meshGameObject == null)
                return;

            var meshRenderer = meshGameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                return;

            meshRenderer.shadowCastingMode = model.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            if (meshRenderer.sharedMaterial != material)
            {
                MaterialTransitionController
                    matTransition = meshGameObject.GetComponent<MaterialTransitionController>();

                if (matTransition != null && matTransition.canSwitchMaterial)
                {
                    matTransition.finalMaterials = new Material[] {material};
                    matTransition.PopulateTargetRendererWithMaterial(matTransition.finalMaterials);
                }

                SRPBatchingHelper.OptimizeMaterial(material);
                meshRenderer.sharedMaterial = material;
            }
        }

        private void OnShapeUpdated(DecentralandEntity entity)
        {
            if (entity != null)
                InitMaterial(entity.meshRootGameObject);
        }

        void OnMaterialDetached(DecentralandEntity entity)
        {
            if (entity.meshRootGameObject == null)
                return;

            entity.OnShapeUpdated -= OnShapeUpdated;

            var meshRenderer = entity.meshRootGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer && meshRenderer.sharedMaterial == material)
                meshRenderer.sharedMaterial = null;
        }

        public override void Dispose()
        {
            dclTexture?.DetachFrom(this);

            Object.Destroy(material);

            base.Dispose();
        }
    }
}