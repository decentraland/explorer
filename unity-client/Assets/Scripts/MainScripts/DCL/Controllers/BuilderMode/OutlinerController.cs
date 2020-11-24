using DCL;
using DCL.Configuration;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinerController : MonoBehaviour
{
    public Material outlineMaterial;
   
    List<DecentralandEntityToEdit> entitiesOutlined = new List<DecentralandEntityToEdit>();

    public void OutlineEntities(List<DecentralandEntityToEdit> entitiesToEdit)
    {
        foreach(DecentralandEntityToEdit entityToEdit in entitiesToEdit)
        {
            OutlineEntity(entityToEdit);
        }
    }

    public void OutlineEntity(DecentralandEntityToEdit entity)
    {
        if (!entity.rootEntity.meshRootGameObject && entity.rootEntity.renderers.Length <= 0) return;

        if (entitiesOutlined.Contains(entity)) return;

        entitiesOutlined.Add(entity);
        if (entity.IsLocked)
            return;
        for (int i = 0; i < entity.rootEntity.meshesInfo.renderers.Length; i++)
        {
            entity.rootEntity.meshesInfo.renderers[i].gameObject.layer = BuilderInWorldSettings.SELECTION_LAYER;
        }
    }

    public void CancelUnselectedOutlines()
    {
        for (int i = 0; i < entitiesOutlined.Count; i++)
        {
            if (!entitiesOutlined[i].IsSelected)
            {
                CancelEntityOutline(entitiesOutlined[i]);
            }
        }
    }

    public void CancelAllOutlines()
    {
        for (int i = 0; i < entitiesOutlined.Count; i++)
        {
            CancelEntityOutline(entitiesOutlined[i]);           
        }
    }

    public void CancelEntityOutline(DecentralandEntityToEdit entityToQuitOutline)
    {
        if (!entitiesOutlined.Contains(entityToQuitOutline)) return;

        if (entityToQuitOutline.rootEntity.meshRootGameObject && entityToQuitOutline.rootEntity.meshesInfo.renderers.Length > 0)
        {
            for (int x = 0; x < entityToQuitOutline.rootEntity.meshesInfo.renderers.Length; x++)
            {
                entityToQuitOutline.rootEntity.meshesInfo.renderers[x].gameObject.layer = BuilderInWorldSettings.DEFAULT_LAYER;
            }
        }
        entitiesOutlined.Remove(entityToQuitOutline);

    }

}
