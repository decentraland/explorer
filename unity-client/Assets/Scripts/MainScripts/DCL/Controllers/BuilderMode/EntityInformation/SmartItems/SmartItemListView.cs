using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartItemListView : MonoBehaviour
{

    [Header("Parameters")]
    [SerializeField] private SmartItemParameterFactory factory;

    List<DCLBuilderInWorldEntity> entitiesList = new List<DCLBuilderInWorldEntity>();

    List<GameObject> childrenList = new List<GameObject>();

    public void SetSmartItemParameters(SmartItemComponent smartItemComponent)
    {
        SetSmartItemParameters(smartItemComponent.model.parameters);
    }

    public void SetSmartItemParameters(SmartItemParameter[] parameters)
    {
        for(int i = 0; i <childrenList.Count;i++)
        {
            Destroy(childrenList[i]);
        }

        gameObject.SetActive(true);

        foreach (SmartItemParameter parameter in parameters)
        {
            SmartItemUIParameterAdapter prefabToInstantiate = factory.GetPrefab(parameter.GetParameterType());
            InstantiateParameter(parameter, prefabToInstantiate);
        }

    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        this.entitiesList = entitiesList;
    }

    void InstantiateParameter(SmartItemParameter parameter, SmartItemUIParameterAdapter parameterAdapterPrefab)
    {
        SmartItemUIParameterAdapter parameterAdapter = Instantiate(parameterAdapterPrefab.gameObject, transform).GetComponent<SmartItemUIParameterAdapter>();

        IEntityListHandler entityListHanlder = parameterAdapter.GetComponent<IEntityListHandler>();
        if(entityListHanlder != null)
            entityListHanlder.SetEntityList(entitiesList);

        parameterAdapter.SetParameter(parameter);
        childrenList.Add(parameterAdapter.gameObject);
    }

    void OnParameterChange(SmartItemParameter smartItemParameter)
    {
        //TODO: Implement Smart Item action
    }
}
