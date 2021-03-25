using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using System;
using DCL;
using UnityEngine.EventSystems;
using DCL.Configuration;

public class CatalogItemAdapter : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public RawImage thumbnailImg;
    public Image backgroundImg;
    public CanvasGroup canvasGroup;
    public GameObject lockedGO;

    [Header("Smart Items")]
    public GameObject smartItemGO;
    public Color smartItemColor, normalColor;

    [Header("Favorites")]
    public Color offFavoriteColor, onFavoriteColor;
    public Image favImg;

    public System.Action<CatalogItem> OnCatalogItemClicked;
    public System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;
    public System.Action<CatalogItem, CatalogItemAdapter, BaseEventData> OnAdapterStartDrag;
    public System.Action<PointerEventData> OnAdapterDrag, OnAdapterEndDrag;
    public System.Action<PointerEventData, CatalogItemAdapter> OnPointerEnterInAdapter;
    public System.Action<PointerEventData, CatalogItemAdapter> OnPointerExitInAdapter;

    private CatalogItem catalogItem;

    private string loadedThumbnailURL;
    private AssetPromise_Texture loadedThumbnailPromise;

    private const float ADAPTER_DRAGGING_SIZE_SCALE = 0.75f;

    public CatalogItem GetContent() { return catalogItem; }

    public void SetContent(CatalogItem catalogItem)
    {
        this.catalogItem = catalogItem;

        if (favImg != null)
            favImg.color = catalogItem.IsFavorite() ? onFavoriteColor : offFavoriteColor;

        if (backgroundImg != null)
            backgroundImg.color = catalogItem.IsSmartItem() ? smartItemColor : normalColor;
        smartItemGO.SetActive(catalogItem.IsSmartItem());

        GetThumbnail();

        lockedGO.gameObject.SetActive(false);

        if (catalogItem.IsNFT() && BuilderInWorldNFTController.i.IsNFTInUse(catalogItem.id))
            lockedGO.gameObject.SetActive(true);
    }

    private void GetThumbnail()
    {
        var url = catalogItem?.GetThumbnailUrl();

        if (url == loadedThumbnailURL)
            return;

        if (catalogItem == null || string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise =  new AssetPromise_Texture(url);


        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);


        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
        loadedThumbnailURL = newLoadedThumbnailURL;
    }

    public void EnableDragMode(Vector2 sizeDelta)
    {
        RectTransform newAdapterRT = GetComponent<RectTransform>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        newAdapterRT.sizeDelta = sizeDelta * ADAPTER_DRAGGING_SIZE_SCALE;
    }

    public void SetFavorite(bool isOn)
    {
        if (isOn)
            favImg.color = onFavoriteColor;
        else
            favImg.color = offFavoriteColor;
    }

    public void AdapterStartDragging(BaseEventData baseEventData) { OnAdapterStartDrag?.Invoke(catalogItem, this, baseEventData); }

    public void FavoriteIconClicked() { OnCatalogItemFavorite?.Invoke(catalogItem, this); }

    public void SceneObjectClicked()
    {
        if (!lockedGO.gameObject.activeSelf)
            OnCatalogItemClicked?.Invoke(catalogItem);
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (thumbnailImg != null)
        {
            thumbnailImg.enabled = true;
            thumbnailImg.texture = texture.texture;
            favImg.gameObject.SetActive(true);

            if (gameObject.activeInHierarchy && ItemAdapterIsOnScreen())
                AudioScriptableObjects.listItemAppear.Play();
        }
    }

    public void OnBeginDrag(PointerEventData eventData) { AdapterStartDragging(eventData); }

    public void OnDrag(PointerEventData eventData) { OnAdapterDrag?.Invoke(eventData); }

    public void OnEndDrag(PointerEventData eventData) { OnAdapterEndDrag?.Invoke(eventData); }

    public void OnPointerEnter(PointerEventData eventData) { OnPointerEnterInAdapter?.Invoke(eventData, this); }

    public void OnPointerExit(PointerEventData eventData) { OnPointerExitInAdapter?.Invoke(eventData, this); }

    private bool ItemAdapterIsOnScreen() {
        return (transform.position.y > 0 && transform.position.y < Screen.height);
    }
}