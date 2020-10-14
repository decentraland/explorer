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
        SHOW = 3,
        DUPLICATE = 4,
    }

    public System.Action<DecentralandEntityToEdit> OnEntityClick,OnEntityDuplicate,OnEntityDelete;
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

    public void OpenEntityList(List<DecentralandEntityToEdit> sceneEntities)
    {
        entitiesList = sceneEntities;
        gameObject.SetActive(true);
        entityListView.gameObject.SetActive(true);
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
                entityToApply.IsLocked = !entityToApply.IsLocked;
                break;
            case EntityAction.DELETE:
              
                OnEntityDelete?.Invoke(entityToApply);
                break;
            case EntityAction.SHOW:
                entityToApply.rootEntity.gameObject.SetActive(!entityToApply.gameObject.activeSelf);
                break;
            case EntityAction.DUPLICATE:
                OnEntityDuplicate?.Invoke(entityToApply);
                //DecentralandEntity newEntity = currentScene.CreateEntity(Guid.NewGuid().ToString());
                //CopyFromEntity(entityToApply.rootEntity, newEntity);
                break;
        }
        //adapter.SetContent(entityToApply);
        //entityListView.SetContent(entitiesList);
    }


    //void CopyFromEntity(DecentralandEntity originalEntity,DecentralandEntity destinationEntity)
    //{
    //    Instantiate(originalEntity.gameObject, destinationEntity.gameObject.transform);
    //}
}
