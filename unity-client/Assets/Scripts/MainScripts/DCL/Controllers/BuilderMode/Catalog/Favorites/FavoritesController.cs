using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavoritesController 
{
    List<CatalogItem> favoritesSceneObjects = new List<CatalogItem>();

    public CatalogGroupListView catalogGroupListView;

    public FavoritesController(CatalogGroupListView catalogGroupListView)
    {
        catalogGroupListView.OnCatalogItemFavorite += ToggleFavoriteState;
    }

    public List<CatalogItem> GetFavorites()
    {
        return favoritesSceneObjects;
    }

    public void ToggleFavoriteState(CatalogItem catalogItem, CatalogItemAdapter adapter)
    {
        if (!favoritesSceneObjects.Contains(catalogItem))
        {
            favoritesSceneObjects.Add(catalogItem);
            catalogItem.SetFavorite(true);
        }
        else
        {
            favoritesSceneObjects.Remove(catalogItem);
            catalogItem.SetFavorite(false);
        }

        adapter.SetFavorite(catalogItem.IsFavorite());
    }
}
