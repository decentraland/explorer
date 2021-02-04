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

    public void ToggleFavoriteState(CatalogItem sceneObject, CatalogItemAdapter adapter)
    {
        if (!favoritesSceneObjects.Contains(sceneObject))
        {
            favoritesSceneObjects.Add(sceneObject);
            sceneObject.SetFavorite(true);
        }
        else
        {
            favoritesSceneObjects.Remove(sceneObject);
            sceneObject.SetFavorite(false);
        }

        adapter.SetFavorite(sceneObject.IsFavorite());
    }
}
