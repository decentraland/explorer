using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityListView : ListView<DecentrelandEntityToEdit>
{
    public EntityListAdapter entityListAdapter;

    public System.Action<BuildModeEntityListController.EntityAction, DecentrelandEntityToEdit, EntityListAdapter> OnActioninvoked;


    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (DecentrelandEntityToEdit entity in contentList)
        {
            EntityListAdapter adapter = Instantiate(entityListAdapter, contentPanelTransform).GetComponent<EntityListAdapter>();
            adapter.SetContent(entity);
            adapter.OnActioninvoked += EntityActionInvoked;
        }
    }

    public void EntityActionInvoked(BuildModeEntityListController.EntityAction action, DecentrelandEntityToEdit entityToApply,EntityListAdapter adapter)
    {
        OnActioninvoked?.Invoke(action, entityToApply,adapter);
    }
}
