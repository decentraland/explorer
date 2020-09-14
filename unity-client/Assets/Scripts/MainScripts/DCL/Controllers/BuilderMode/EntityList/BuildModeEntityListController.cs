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

    public EntityListView entityListView;
    ParcelScene currentScene;

    private void Awake()
    {
        entityListView.OnActioninvoked += EntityActionInvoked;
    }

    private void OnDestroy()
    {
        entityListView.OnActioninvoked -= EntityActionInvoked;
    }

    public void OpenEntityList(ParcelScene scene)
    {
        gameObject.SetActive(true);
        currentScene = scene;
        entityListView.gameObject.SetActive(true);
        entityListView.SetContent(scene.entities.Values.ToList());
    }

    public void CloseList()
    {
        gameObject.SetActive(false);
        entityListView.gameObject.SetActive(false);
    }

    public void EntityActionInvoked(EntityAction action, DecentralandEntity entityToApply)
    {
        switch (action)
        {
            case EntityAction.SELECT:
                break;
            case EntityAction.LOCK:
                entityToApply.isLocked = !entityToApply.isLocked;
                break;
            case EntityAction.DELETE:
                currentScene.RemoveEntity(entityToApply.entityId);
                break;
            case EntityAction.SHOW:
                entityToApply.gameObject.SetActive(!entityToApply.gameObject.activeSelf);
                break;
            case EntityAction.DUPLICATE:
                DecentralandEntity newEntity = currentScene.CreateEntity(Guid.NewGuid().ToString());
                CopyFromEntity(entityToApply, newEntity);
                break;
        }
    }


    void CopyFromEntity(DecentralandEntity originalEntity,DecentralandEntity destinationEntity)
    {
        Instantiate(originalEntity.gameObject, destinationEntity.gameObject.transform);
    }
}
