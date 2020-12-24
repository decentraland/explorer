using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartItemListView : MonoBehaviour
{
    public SmartItemUIParameterAdapter parameterAdapterPrefab;
    public SmartItemUIAdapter actionAdapter;


    List<GameObject> childrenList = new List<GameObject>();

    public void SetSmartItem(SmartItemComponent smartItemComponent)
    {
        for(int i = 0; i <childrenList.Count;i++)
        {
            Destroy(childrenList[i]);
        }

        gameObject.SetActive(true);

        foreach (SmartItemParameter parameter in smartItemComponent.model.parameters)
        {
            SmartItemUIParameterAdapter parameterAdapter = Instantiate(parameterAdapterPrefab.gameObject, transform).GetComponent< SmartItemUIParameterAdapter>();
            parameterAdapter.SetParameter(parameter);
            childrenList.Add(parameterAdapter.gameObject);
        }

        foreach (SmartItemAction action in smartItemComponent.model.actions)
        {

        }
    }
}
