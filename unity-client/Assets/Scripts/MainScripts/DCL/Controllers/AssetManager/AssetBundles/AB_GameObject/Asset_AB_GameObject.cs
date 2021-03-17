using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;

namespace DCL
{
    public class Asset_AB_GameObject : Asset_WithPoolableContainer
    {
        internal AssetPromise_AB ownerPromise;
        public override GameObject container { get; set; }
        public bool isInstantiated;
        private bool visible = true;

        public Asset_AB_GameObject()
        {
            isInstantiated = false;
            container = new GameObject("AB Container");
            // Hide gameobject until it's been correctly processed, otherwise it flashes at 0,0,0
            container.transform.position = EnvironmentSettings.MORDOR;
        }

        public override void Cleanup()
        {
            AssetPromiseKeeper_AB.i.Forget(ownerPromise);
            Object.Destroy(container);
        }

        public void Hide()
        {
            container.transform.parent = null;
            container.transform.position = EnvironmentSettings.MORDOR;
            visible = false;
        }

        private Coroutine showCoroutine;

        public void CancelShow()
        {
            if (showCoroutine != null)
                CoroutineStarter.Stop(showCoroutine);
        }

        public void Show(bool useMaterialTransition, System.Action OnFinish)
        {
            if (showCoroutine != null)
                CoroutineStarter.Stop(showCoroutine);

            if (!visible)
            {
                OnFinish?.Invoke();
                return;
            }

            bool renderingEnabled = CommonScriptableObjects.rendererState.Get();

            if (!renderingEnabled || !useMaterialTransition)
            {
                container.SetActive(true);
                OnFinish?.Invoke();
                return;
            }

            container.SetActive(false);
            showCoroutine = CoroutineStarter.Start(ShowCoroutine(OnFinish));
        }

        public IEnumerator ShowCoroutine(System.Action OnFinish)
        {
            // NOTE(Brian): This fixes seeing the object in the scene 0,0 for a frame
            yield return new WaitForSeconds(Random.Range(0, 0.05f));

            // NOTE(Brian): This GameObject can be removed by distance after the delay
            if (container == null)
            {
                OnFinish?.Invoke();
                yield break;
            }

            container.SetActive(true);
            OnFinish?.Invoke();
        }
    }
}