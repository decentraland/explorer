using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityListView : MonoBehaviour
{
    public Transform contentPanel;

    public EntityListAdapter entityListAdapter;
    public System.Action<BuildModeEntityListController.EntityAction,DecentralandEntity> OnActioninvoked;

    List<DecentralandEntity> entityList;
    public void SetContent(List<DecentralandEntity> _entityList)
    {
        entityList = _entityList;

        foreach (DecentralandEntity entity in entityList)
        {
            EntityListAdapter adapter = Instantiate(entityListAdapter, contentPanel).GetComponent<EntityListAdapter>();
            adapter.SetContent(entity);
            adapter.OnActioninvoked += EntityActionInvoked;
        }
    }
    public void EntityActionInvoked(BuildModeEntityListController.EntityAction action, DecentralandEntity entityToApply)
    {
        OnActioninvoked?.Invoke(action, entityToApply);
    }
}
