using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildModeEntityListController : MonoBehaviour
{
    public enum EntityAction
    {
        SELECT = 0,
        LOCK = 1,
        DELETE = 2,
        SHOW = 3
    }

    public System.Action<DecentralandEntityToEdit> OnEntityClick,OnEntityDelete,OnEntityLock,OnEntityChangeVisibility;
    public EntityListView entityListView;
    ParcelScene currentScene;
    List<DecentralandEntityToEdit> entitiesList;

    private void Awake()
    {
        entityListView.OnActioninvoked += EntityActionInvoked;
    }

    private void OnDestroy()
    {
        entityListView.OnActioninvoked -= EntityActionInvoked;
    }

    public void OpenEntityList(List<DecentralandEntityToEdit> sceneEntities,ParcelScene parcelScene)
    {
        if (currentScene == null)
        {
            currentScene = parcelScene;
            SetEntityList(sceneEntities);
        }
        else
        {
            if (currentScene != parcelScene) SetEntityList(sceneEntities);
        }
        gameObject.SetActive(true);
        entityListView.gameObject.SetActive(true);

    }

    public void SetEntityList(List<DecentralandEntityToEdit> sceneEntities)
    {
        entitiesList = sceneEntities;
        entityListView.SetContent(sceneEntities);
    }

    public void CloseList()
    {
        gameObject.SetActive(false);
        entityListView.gameObject.SetActive(false);
    }

    public void EntityActionInvoked(EntityAction action, DecentralandEntityToEdit entityToApply,EntityListAdapter adapter)
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

}
