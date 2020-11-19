using System;
using DCL;
using DCL.Helpers.NFT;
using UnityEngine;

public class NFTShapeHQImageConfig
{
    public string contentType;
    public NFTInfo nftInfo;
    public NFTShapeConfig nftConfig;
    public NFTShapeLoaderController controller;
    public AssetPromise_Texture previewTexture;
}

public class NFTShapeHQImageHandler : IDisposable
{
    NFTShapeHQImageConfig config;
    AssetPromise_Texture hqTexture;

    bool isPlayerNear;
    Camera camera;

    static public NFTShapeHQImageHandler Create(NFTShapeHQImageConfig config)
    {
        if (config.contentType == "image/gif")
            return null;

        return new NFTShapeHQImageHandler(config);
    }

    public void Dispose()
    {
        CommonScriptableObjects.playerUnityPosition.OnChange -= OnPlayerPositionChanged;

        if (hqTexture != null)
        {
            ForgetHQTexture();
        }
    }

    public void Update()
    {
        if (!isPlayerNear)
            return;

        float dot = Vector3.Dot(config.controller.transform.forward, camera.transform.forward);

        if (dot >= config.nftConfig.highQualityImageAngleRatio && hqTexture == null)
        {
            FetchHQTexture();
        }
    }

    private NFTShapeHQImageHandler(NFTShapeHQImageConfig config)
    {
        this.config = config;
        camera = Camera.main;

        CommonScriptableObjects.playerUnityPosition.OnChange += OnPlayerPositionChanged;
        OnPlayerPositionChanged(CommonScriptableObjects.playerUnityPosition, Vector3.zero);
    }

    private void OnPlayerPositionChanged(Vector3 current, Vector3 prev)
    {
        // NOTE: currently all of our NFTShapes have a collider... but... better to be safe 🤷‍♂️
        if (config.controller.collider != null)
        {
            isPlayerNear = ((current - config.controller.collider.ClosestPointOnBounds(current)).sqrMagnitude
                < (config.nftConfig.highQualityImageMinDistance * config.nftConfig.highQualityImageMinDistance));
        }
        else
        {
            isPlayerNear = ((current - config.controller.transform.position).sqrMagnitude
                < (config.nftConfig.highQualityImageMinDistance * config.nftConfig.highQualityImageMinDistance));
        }

        if (!isPlayerNear && hqTexture != null)
        {
            ForgetHQTexture();
            RestorePreviewTexture();
        }

        if (config.nftConfig.verbose)
        {
            Debug.Log($"Player near {config.nftInfo.name}? {isPlayerNear}");
        }
    }

    private void FetchHQTexture()
    {
        hqTexture = new AssetPromise_Texture(string.Format("{0}=s{1}", config.nftInfo.imageUrl, config.nftConfig.highQualityImageResolution));
        AssetPromiseKeeper_Texture.i.Keep(hqTexture);
        hqTexture.OnSuccessEvent += (asset) => config.controller.UpdateTexture(asset.texture);
        if (config.nftConfig.verbose)
        {
            Debug.Log($"Fetch {config.nftInfo.name} HQ image");
        }
    }

    private void ForgetHQTexture()
    {
        AssetPromiseKeeper_Texture.i.Forget(hqTexture);
        hqTexture = null;
        if (config.nftConfig.verbose)
        {
            Debug.Log($"Forget {config.nftInfo.name} HQ image");
        }
    }

    private void RestorePreviewTexture()
    {
        config.controller.UpdateTexture(config.previewTexture.asset.texture);
        if (config.nftConfig.verbose)
        {
            Debug.Log($"Restore {config.nftInfo.name} preview image");
        }
    }
}
