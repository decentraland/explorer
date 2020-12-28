using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartItemListView : MonoBehaviour
{

    [Header("Parameters")]
    public SmartItemUIParameterAdapter booleanParameterPrefab;
    public SmartItemUIParameterAdapter textParameterPrefab;
    public SmartItemUIParameterAdapter textAreaParameterPrefab;
    public SmartItemUIParameterAdapter floatParameterPrefab;
    public SmartItemUIParameterAdapter integerParameterPrefab;
    public SmartItemUIParameterAdapter sliderParameterPrefab;
    public SmartItemUIParameterAdapter optionsParameterPrefab;
    public SmartItemUIParameterAdapter entityParameterPrefab;
    public SmartItemUIParameterAdapter actionParameterPrefab;

    [Header("Actions")]
    public SmartItemUIAdapter actionAdapter;


    List<DCLBuilderInWorldEntity> entitiesList = new List<DCLBuilderInWorldEntity>();

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
            SmartItemUIParameterAdapter prefabToInstantiate = null;

            switch (parameter.type)
            {
                case "boolean":
                    prefabToInstantiate = booleanParameterPrefab;
                    break;
                case "text":
                    prefabToInstantiate = textParameterPrefab;
                    break;
                case "textarea":
                    prefabToInstantiate = textAreaParameterPrefab;
                    break;
                case "float":
                    prefabToInstantiate = floatParameterPrefab;
                    break;
                case "integer":
                    prefabToInstantiate = integerParameterPrefab;
                    break;
                case "slider":
                    prefabToInstantiate = sliderParameterPrefab;
                    break;
                case "options":
                    prefabToInstantiate = optionsParameterPrefab;
                    break;
                case "entity":
                    prefabToInstantiate = entityParameterPrefab;
                    break;
                case "actions":
                    prefabToInstantiate = actionParameterPrefab;
                    break;
                default:
                    Debug.Log("This parameter doesn't exists!");
                    break;

            }

            InstantiateParameter(parameter, prefabToInstantiate);
        }

        foreach (SmartItemAction action in smartItemComponent.model.actions)
        {

        }
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        this.entitiesList = entitiesList;
    }

    void InstantiateParameter(SmartItemParameter parameter, SmartItemUIParameterAdapter parameterAdapterPrefab)
    {
        SmartItemUIParameterAdapter parameterAdapter = Instantiate(parameterAdapterPrefab.gameObject, transform).GetComponent<SmartItemUIParameterAdapter>();
        parameterAdapter.SetEntityList(entitiesList);
        parameterAdapter.SetParameter(parameter);
        childrenList.Add(parameterAdapter.gameObject);
    }
}
