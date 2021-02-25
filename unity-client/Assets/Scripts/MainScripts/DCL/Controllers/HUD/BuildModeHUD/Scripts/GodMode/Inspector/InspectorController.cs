using System;
using System.Collections.Generic;
using UnityEngine.Events;

public enum EntityAction
{
    SELECT = 0,
    LOCK = 1,
    DELETE = 2,
    SHOW = 3
}

public interface IInspectorController
{
    event Action<DCLBuilderInWorldEntity> OnEntityClick;
    event Action<DCLBuilderInWorldEntity> OnEntityDelete;
    event Action<DCLBuilderInWorldEntity> OnEntityLock;
    event Action<DCLBuilderInWorldEntity> OnEntityChangeVisibility;
    event Action<DCLBuilderInWorldEntity, string> OnEntityRename;

    void Initialize(InspectorView inspectorView);
    void Dispose();
    void OpenEntityList();
    void SetEntityList(List<DCLBuilderInWorldEntity> sceneEntities);
    void ClearList();
    void CloseList();
    void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply, EntityListAdapter adapter);
    void EntityRename(DCLBuilderInWorldEntity entity, string newName);
    void SetCloseButtonsAction(UnityAction call);
}

public class InspectorController : IInspectorController
{
    public event Action<DCLBuilderInWorldEntity> OnEntityClick;
    public event Action<DCLBuilderInWorldEntity> OnEntityDelete;
    public event Action<DCLBuilderInWorldEntity> OnEntityLock;
    public event Action<DCLBuilderInWorldEntity> OnEntityChangeVisibility;
    public event Action<DCLBuilderInWorldEntity, string> OnEntityRename;

    private InspectorView inspectorView;

    public void Initialize(InspectorView inspectorView)
    {
        this.inspectorView = inspectorView;

        inspectorView.OnEntityActionInvoked += EntityActionInvoked;
        inspectorView.OnEntityRename += EntityRename;
    }

    public void Dispose()
    {
        inspectorView.OnEntityActionInvoked -= EntityActionInvoked;
        inspectorView.OnEntityRename -= EntityRename;
    }

    public void OpenEntityList()
    {
        inspectorView.entityListView.SetContent(inspectorView.entitiesList);
        inspectorView.SetActive(true);
        inspectorView.entityListView.SetActive(true);
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> sceneEntities)
    {
        if (inspectorView.entityListView == null)
            return;

        inspectorView.SetEntitiesList(sceneEntities);
        if (inspectorView.entityListView.isActive)
            inspectorView.entityListView.SetContent(inspectorView.entitiesList);
    }

    public void ClearList()
    {
        inspectorView.ClearEntitiesList();
        inspectorView.entityListView.RemoveAdapters();
    }

    public void CloseList()
    {
        inspectorView.SetActive(false);
        inspectorView.entityListView.SetActive(false);
    }

    public void EntityActionInvoked(EntityAction action, DCLBuilderInWorldEntity entityToApply, EntityListAdapter adapter)
    {
        switch (action)
        {
            case EntityAction.SELECT:

                OnEntityClick?.Invoke(entityToApply);
                break;
            case EntityAction.LOCK:

                OnEntityLock?.Invoke(entityToApply);
                break;
            case EntityAction.DELETE:

                OnEntityDelete?.Invoke(entityToApply);
                break;
            case EntityAction.SHOW:
                OnEntityChangeVisibility?.Invoke(entityToApply);
                break;
        }
    }

    public void EntityRename(DCLBuilderInWorldEntity entity, string newName)
    {
        OnEntityRename?.Invoke(entity, newName);
    }

    public void SetCloseButtonsAction(UnityAction call)
    {
        inspectorView.SetCloseButtonsAction(call);
    }
}
