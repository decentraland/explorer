using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;

namespace DCL
{
    public class Asset_AB_GameObject : Asset_WithPoolableContainer
    {
        internal AssetPromise_AB ownerPromise;
        public override GameObject container { get; set; }
        public override bool hasAnimation { get; set; }

        public Asset_AB_GameObject()
        {
            container = new GameObject("AB Container");
            // Hide gameobject until it's been correctly processed, otherwise it flashes at 0,0,0
            container.transform.position = EnvironmentSettings.MORDOR;
        }

        public override void Cleanup()
        {
            AssetPromiseKeeper_AB.i.Forget(ownerPromise);
            Object.Destroy(container);
        }

        public void Show(System.Action OnFinish)
        {
            OnFinish?.Invoke();
        }

        public void Hide()
        {
            container.transform.parent = null;
            container.transform.position = EnvironmentSettings.MORDOR;
        }
    }
}