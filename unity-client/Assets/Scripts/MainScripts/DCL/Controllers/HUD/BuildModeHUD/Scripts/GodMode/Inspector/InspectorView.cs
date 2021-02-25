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
    [SerializeField] internal SceneLimitsView sceneLimitsView;
    [SerializeField] internal Button[] closeEntityListBtns;

    internal List<DCLBuilderInWorldEntity> entitiesList;
    internal ISceneLimitsController sceneLimitsController;

    private void Awake()
    {
        entityListView.OnActionInvoked += EntityActionInvoked;
        entityListView.OnEntityRename += EntityRename;
    }

    private void OnDestroy()
    {
        entityListView.OnActionInvoked -= EntityActionInvoked;
        entityListView.OnEntityRename -= EntityRename;

        if (sceneLimitsController != null)
            sceneLimitsController.Dispose();
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

    public void ConfigureSceneLimits(ISceneLimitsController sceneLimitsController)
    {
        this.sceneLimitsController = sceneLimitsController;
        this.sceneLimitsController.Initialize(sceneLimitsView);
    }
}
