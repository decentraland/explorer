using DCL;
using DCL.Interface;
using UnityEngine;
using UnityGLTF;

public class RenderingController : MonoBehaviour
{
    public static RenderingController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    public System.Action<bool> OnRenderingStateChanged;
    public bool renderingEnabled { get; private set; } = true;

    [ContextMenu("Disable Rendering")]
    public void DeactivateRendering()
    {
        if (!renderingEnabled)
            return;

        renderingEnabled = false;

        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        MessagingBus.renderingIsDisabled = true;
        PointerEventsController.renderingIsDisabled = true;
        InputController.renderingIsDisabled = true;
        GLTFSceneImporter.renderingIsDisabled = true;

        AssetPromiseKeeper_GLTF.i.useBlockedPromisesQueue = false;
        AssetPromiseKeeper_AssetBundle.i.useBlockedPromisesQueue = false;
        AssetPromiseKeeper_AssetBundleModel.i.useBlockedPromisesQueue = false;

        DCLCharacterController.i.SetEnabled(false);

        OnRenderingStateChanged?.Invoke(renderingEnabled);
    }

    [ContextMenu("Enable Rendering")]
    public void ActivateRendering()
    {
        if (renderingEnabled)
            return;

        renderingEnabled = true;

        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = true;
        MessagingBus.renderingIsDisabled = false;
        GLTFSceneImporter.renderingIsDisabled = false;
        PointerEventsController.renderingIsDisabled = false;
        InputController.renderingIsDisabled = false;
        DCLCharacterController.i.SetEnabled(true);

        AssetPromiseKeeper_GLTF.i.useBlockedPromisesQueue = true;
        AssetPromiseKeeper_AssetBundle.i.useBlockedPromisesQueue = true;
        AssetPromiseKeeper_AssetBundleModel.i.useBlockedPromisesQueue = true;

        OnRenderingStateChanged?.Invoke(renderingEnabled);

        WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }
}
