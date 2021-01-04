using DCL;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoCollectibleItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;

    internal WearableItem collectible;
    private AssetPromise_Texture thumbnailPromise;
    private Sprite placeholderSprite;

    public void Initialize(WearableItem collectible)
    {
        placeholderSprite = thumbnail.sprite;
        this.collectible = collectible;

        if (this.collectible == null) return;

        if (gameObject.activeInHierarchy)
            GetThumbnail();
    }

    private void OnEnable()
    {
        if (collectible == null) return;

        GetThumbnail();
    }

    private void OnDisable()
    {
        if (collectible != null)
            ForgetThumbnail();
    }

    private void GetThumbnail()
    {
        string url = collectible.ComposeThumbnailUrl();
        //NOTE(Brian): Get before forget to prevent referenceCount == 0 and asset unload
        var newThumbnailPromise = ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
        ForgetThumbnail();
        thumbnailPromise = newThumbnailPromise;
    }

    private void ForgetThumbnail()
    {
        ThumbnailsManager.ForgetThumbnail(thumbnailPromise);
    }

    private void OnThumbnailReady(Asset_Texture texture)
    {
        // we can't destroy the referenced asset placeholder sprite or we get the "Destroying assets it not permitted to avoid data loss" error
        if (thumbnail.sprite != null && thumbnail.sprite != placeholderSprite)
            Destroy(thumbnail.sprite);

        thumbnail.sprite = ThumbnailsManager.CreateSpriteFromTexture(texture.texture);
    }
}