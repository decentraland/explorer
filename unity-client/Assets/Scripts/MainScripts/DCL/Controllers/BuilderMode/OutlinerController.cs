using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinerController : MonoBehaviour
{
    public GameObject outlinerPrefab;


    List<MeshFilter> outliners = new List<MeshFilter>();
    public void OutLineOnlyThisEntity(DecentralandEntity entity)
    {

        for(int i = 0; i < entity.meshesInfo.renderers.Length; i++)
        {
            if (i >= outliners.Count) outliners.Add(Instantiate(outlinerPrefab).GetComponent<MeshFilter>());
            else
            {
                outliners[i].gameObject.SetActive(true);
                outliners[i].mesh = entity.meshesInfo.renderers[i].GetComponent<MeshFilter>().mesh;
                outliners[i].gameObject.transform.SetParent(entity.meshesInfo.renderers[i].gameObject.transform.parent, false);
            }
        }
        for(int i = entity.meshesInfo.meshFilters.Length; i < outliners.Count; i++)
        {
            outliners[i].gameObject.SetActive(false);
        }
       
 
    }


    public void CancelAllOutlines()
    {
        foreach(MeshFilter filter in outliners)
        {
            filter.gameObject.SetActive(false);
        }
    }
}
