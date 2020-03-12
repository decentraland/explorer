using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using UnityGLTF;
public class RenderingController : MonoBehaviour
{
    public CompositeLock renderingActivatedAckLock = new CompositeLock();

    private bool activatedRenderingBefore { get; set; } = false;

    void Awake()
    {
        CommonScriptableObjects.rendererState.OnLockAdded += AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved += RemoveLock;
    }

    void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnLockAdded -= AddLock;
        CommonScriptableObjects.rendererState.OnLockRemoved -= RemoveLock;
    }

    [ContextMenu("Disable Rendering")]
    public void DeactivateRendering()
    {
        if (!CommonScriptableObjects.rendererState.Get())
            return;

        DeactivateRendering_Internal();
    }

    void DeactivateRendering_Internal()
    {
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        MessagingBus.renderingIsDisabled = true;
        PointerEventsController.renderingIsDisabled = true;
        InputController_Legacy.renderingIsDisabled = true;
        GLTFSceneImporter.renderingIsDisabled = true;

        AssetPromiseKeeper_GLTF.i.useTimeBudget = false;
        AssetPromiseKeeper_AB.i.useTimeBudget = false;
        AssetPromiseKeeper_AB_GameObject.i.useTimeBudget = false;
        AssetPromise_AB.limitTimeBudget = false;

        DCLCharacterController.i.SetEnabled(false);

        CommonScriptableObjects.rendererState.Set(false);
    }


    [ContextMenu("Enable Rendering")]
    public void ActivateRendering()
    {
        if (CommonScriptableObjects.rendererState.Get())
            return;

        if (!renderingActivatedAckLock.isUnlocked)
        {
            renderingActivatedAckLock.OnAllLocksRemoved -= ActivateRendering_Internal;
            renderingActivatedAckLock.OnAllLocksRemoved += ActivateRendering_Internal;
            return;
        }

        ActivateRendering_Internal();
    }

    private void ActivateRendering_Internal()
    {
        renderingActivatedAckLock.OnAllLocksRemoved -= ActivateRendering_Internal;

        if (!activatedRenderingBefore)
        {
            Utils.UnlockCursor();
            activatedRenderingBefore = true;
        }

        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = true;
        MessagingBus.renderingIsDisabled = false;
        GLTFSceneImporter.renderingIsDisabled = false;
        PointerEventsController.renderingIsDisabled = false;
        InputController_Legacy.renderingIsDisabled = false;
        DCLCharacterController.i.SetEnabled(true);

        AssetPromise_AB.limitTimeBudget = true;

        AssetPromiseKeeper_GLTF.i.useTimeBudget = true;
        AssetPromiseKeeper_AB.i.useTimeBudget = true;
        AssetPromiseKeeper_AB_GameObject.i.useTimeBudget = true;

        CommonScriptableObjects.rendererState.Set(true);

        MemoryManager.i.CleanupPoolsIfNeeded(true);
        ParcelScene.parcelScenesCleaner.ForceCleanup();

        WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }

    private void AddLock(object id)
    {
        renderingActivatedAckLock.AddLock(id);
    }

    private void RemoveLock(object id)
    {
        renderingActivatedAckLock.RemoveLock(id);
    }
}
