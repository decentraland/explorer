using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityListView : ListView<DecentralandEntity>
{
    public EntityListAdapter entityListAdapter;

    public System.Action<BuildModeEntityListController.EntityAction,DecentralandEntity> OnActioninvoked;


    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (DecentralandEntity entity in contentList)
        {
            EntityListAdapter adapter = Instantiate(entityListAdapter, contentPanelTransform).GetComponent<EntityListAdapter>();
            adapter.SetContent(entity);
            adapter.OnActioninvoked += EntityActionInvoked;
        }
    }

    public void EntityActionInvoked(BuildModeEntityListController.EntityAction action, DecentralandEntity entityToApply)
    {
        OnActioninvoked?.Invoke(action, entityToApply);
    }
}
