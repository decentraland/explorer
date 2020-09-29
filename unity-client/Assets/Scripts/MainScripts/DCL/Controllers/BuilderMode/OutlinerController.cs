using DCL;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinerController : MonoBehaviour
{
    public GameObject outlinerPrefab;


    List<MeshFilter> outliners = new List<MeshFilter>();

    public DecentralandEntity currentSelectedEntity;
    public void OutLineOnlyThisEntity(DecentralandEntity entity)
    {
        if (currentSelectedEntity != null)
        {
            currentSelectedEntity.OnCleanupEvent -= CleanOutLinersFromClean;
            currentSelectedEntity.OnRemoved -= CleanOutlinersFromEntity;
        }
        entity.OnCleanupEvent += CleanOutLinersFromClean;
        entity.OnRemoved += CleanOutlinersFromEntity;
        currentSelectedEntity = entity;
        foreach (MeshFilter meshFilter in outliners)
        {
            if(meshFilter != null)meshFilter.gameObject.SetActive(false);
        }
    
        if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0)
        {
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                if (i >= outliners.Count) outliners.Add(Instantiate(outlinerPrefab).GetComponent<MeshFilter>());
                else
                {
                    if (outliners[i] == null) outliners[i] = Instantiate(outlinerPrefab).GetComponent<MeshFilter>();
                    outliners[i].gameObject.SetActive(true);
                    outliners[i].mesh = entity.meshesInfo.renderers[i].GetComponent<MeshFilter>().mesh;
                    outliners[i].gameObject.transform.SetParent(entity.meshesInfo.renderers[i].gameObject.transform.parent, false);
                }
            }
        }
    }

    void CleanOutLinersFromClean(ICleanableEventDispatcher dispatcher)
    {
        CleanOutLiners();
    }
    void CleanOutlinersFromEntity(DecentralandEntity entity)
    {
        CleanOutLiners();
    }
    void CleanOutLiners()
    {
        for(int i = 0; i <outliners.Count;i++)
        {
            if(outliners[i] == null) outliners.Remove(outliners[i]);
        }
    }


    public void CancelAllOutlines()
    {
        foreach(MeshFilter filter in outliners)
        {
            if (filter != null) filter.gameObject.SetActive(false);
        }
    }
}
