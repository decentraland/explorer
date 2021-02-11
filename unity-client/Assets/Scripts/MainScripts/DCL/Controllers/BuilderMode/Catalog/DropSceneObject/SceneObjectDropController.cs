using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneObjectDropController
{
    public CatalogGroupListView catalogGroupListView;
    public event Action<CatalogItem> OnCatalogItemDropped;

    public void SceneObjectDropped()
    {
        CatalogItemAdapter adapter = catalogGroupListView.GetLastSceneObjectDragged();
        if (adapter == null)
            return;
        CatalogItem catalogItem = adapter.GetContent();

        OnCatalogItemDropped?.Invoke(catalogItem);
    }
}
