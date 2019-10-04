using UnityEngine;
using DCL.Helpers;

namespace Builder
{
    public class DCLBuilderSelectionCollider : MonoBehaviour
    {
        public const string LAYER_BUILDER_POINTER_CLICK = "OnBuilderPointerClick";

        public DCLBuilderEntity ownerEntity { get; private set; }

        public void Initialize(DCLBuilderEntity builderEntity, Renderer renderer)
        {
            ownerEntity = builderEntity;

            gameObject.layer = LayerMask.NameToLayer(LAYER_BUILDER_POINTER_CLICK);

            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.enabled = renderer.enabled;

            Transform t = gameObject.transform;

            t.SetParent(renderer.transform);
            Utils.ResetLocalTRS(t);
        }
    }
}