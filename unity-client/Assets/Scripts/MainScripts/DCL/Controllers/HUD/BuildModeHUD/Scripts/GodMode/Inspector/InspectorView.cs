using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InspectorView : MonoBehaviour
{
    internal event Action<EntityAction, DCLBuilderInWorldEntity, EntityListAdapter> OnEntityActionInvoked;
    internal event Action<DCLBuilderInWorldEntity, string> OnEntityRename;

    [SerializeField] internal EntityListView entityListView;
    [SerializeField] internal Button[] closeEntityListBtns;

    internal List<DCLBuilderInWorldEntity> entitiesList;

    private void Awake()
    {
        entityListView.OnActionInvoked += EntityActionInvoked;
        entityListView.OnEntityRename += EntityRename;
    }

    private void OnDestroy()
    {
        entityListView.OnActionInvoked -= EntityActionInvoked;
        entityListView.OnEntityRename -= EntityRename;
    }

    private void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply, EntityListAdapter adapter)
    {
        OnEntityActionInvoked?.Invoke(action, entityToApply, adapter);
    }

    private void EntityRename(DCLBuilderInWorldEntity entity, string newName)
    {
        OnEntityRename?.Invoke(entity, newName);
    }

    internal void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    internal void SetEntitiesList(List<DCLBuilderInWorldEntity> entities)
    {
        entitiesList = entities;
    }

    internal void ClearEntitiesList()
    {
        entitiesList.Clear();
    }

    public void SetCloseButtonsAction(UnityAction call)
    {
        foreach (Button closeEntityListBtn in closeEntityListBtns)
        {
            closeEntityListBtn.onClick.AddListener(call);
        }
    }
}
