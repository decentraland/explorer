using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityListView : ListView<DecentralandEntityToEdit>
{
    public EntityListAdapter entityListAdapter;

    public System.Action<BuildModeEntityListController.EntityAction, DecentralandEntityToEdit, EntityListAdapter> OnActioninvoked;


    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (DecentralandEntityToEdit entity in contentList)
        {
            EntityListAdapter adapter = Instantiate(entityListAdapter, contentPanelTransform).GetComponent<EntityListAdapter>();
            adapter.SetContent(entity);
            adapter.OnActionInvoked += EntityActionInvoked;
        }
    }

    public void EntityActionInvoked(BuildModeEntityListController.EntityAction action, DecentralandEntityToEdit entityToApply,EntityListAdapter adapter)
    {
        OnActioninvoked?.Invoke(action, entityToApply,adapter);
    }
}
