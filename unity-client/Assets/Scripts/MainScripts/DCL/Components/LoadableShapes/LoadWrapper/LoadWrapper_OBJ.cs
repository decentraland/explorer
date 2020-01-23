using DCL.Helpers;

namespace DCL.Components
{
    public class LoadWrapper_OBJ : LoadWrapper
    {
        DynamicOBJLoaderController objLoaderComponent;

        public override void Unload()
        {
            objLoaderComponent.OnFinishedLoadingAsset -= CallOnComponentUpdated;
            UnityEngine.Object.Destroy(objLoaderComponent);
            entity.Cleanup();
        }

        public override void Load(string src, System.Action<LoadWrapper> OnSuccess, System.Action<LoadWrapper> OnFail)
        {
            if (string.IsNullOrEmpty(src))
                return;

            if (objLoaderComponent == null)
                objLoaderComponent = entity.meshRootGameObject.GetOrCreateComponent<DynamicOBJLoaderController>();

            objLoaderComponent.OnFinishedLoadingAsset += CallOnComponentUpdated;

            alreadyLoaded = false;
            objLoaderComponent.OnFinishedLoadingAsset += () => OnSuccess(this);
            objLoaderComponent.LoadAsset(src, true);

            if (objLoaderComponent.loadingPlaceholder == null)
            {
                objLoaderComponent.loadingPlaceholder =
                    Helpers.Utils.AttachPlaceholderRendererGameObject(entity.gameObject.transform);
            }
            else
            {
                objLoaderComponent.loadingPlaceholder.SetActive(true);
            }
        }

        void CallOnComponentUpdated()
        {
            alreadyLoaded = true;

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }

            CollidersManager.i.ConfigureColliders(entity);
        }

    }
}
