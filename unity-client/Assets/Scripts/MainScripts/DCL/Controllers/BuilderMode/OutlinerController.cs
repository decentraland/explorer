using DCL;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinerController : MonoBehaviour
{

    List<DecentralandEntityToEdit> entitiesOutlined = new List<DecentralandEntityToEdit>();


    public void OutlineEntities(List<DecentralandEntityToEdit> entitiesToEdit)
    {
        foreach(DecentralandEntityToEdit entityToEdit in entitiesToEdit)
        {
            OutLineEntity(entityToEdit);
        }
    }

    public void OutLineEntity(DecentralandEntityToEdit entity)
    {

        if (entity.rootEntity.meshRootGameObject && entity.rootEntity.renderers.Length > 0)
        {
            if (!entitiesOutlined.Contains(entity))
            {
                entitiesOutlined.Add(entity);
                for (int i = 0; i < entity.rootEntity.meshesInfo.renderers.Length; i++)
                {
                    entity.rootEntity.meshesInfo.renderers[i].gameObject.layer = LayerMask.NameToLayer("Selection");
                }
            }
        }

    }


    public void CancelUnselectedOutlines()
    {
        for (int i = 0; i < entitiesOutlined.Count; i++)
        {
            if (!entitiesOutlined[i].IsSelected)
            {
                CancelEntityOutline(entitiesOutlined[i]);
                entitiesOutlined.Remove(entitiesOutlined[i]);
            }
        }
    }
    public void CancelAllOutlines()
    {
        for (int i = 0; i < entitiesOutlined.Count; i++)
        {
            CancelEntityOutline(entitiesOutlined[i]);
            entitiesOutlined.Remove(entitiesOutlined[i]);
           
        }
    }

    public void CancelEntityOutline(DecentralandEntityToEdit entityToQuitOutline)
    {
        if (entitiesOutlined.Contains(entityToQuitOutline))
        {
            if (entityToQuitOutline.rootEntity.meshRootGameObject && entityToQuitOutline.rootEntity.meshesInfo.renderers.Length > 0)
            {
                for (int x = 0; x < entityToQuitOutline.rootEntity.meshesInfo.renderers.Length; x++)
                {
                    entityToQuitOutline.rootEntity.meshesInfo.renderers[x].gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }
}
