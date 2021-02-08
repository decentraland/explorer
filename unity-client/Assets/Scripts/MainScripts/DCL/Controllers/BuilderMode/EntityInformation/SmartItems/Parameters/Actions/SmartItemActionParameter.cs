using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemActionParameter : SmartItemUIParameterAdapter, IEntityListHandler
{
    public ActionsListView actionsListView;
    public Button addActionBtn;

    List<DCLBuilderInWorldEntity> entitiesList;

    private void Start()
    {
        addActionBtn.onClick.AddListener(AddEventAction);
    }

    public void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        this.entitiesList = entitiesList;
    }

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);
        //TODO include the functionality of the parameter
    }


    public void AddEventAction()
    {
        List<DCLBuilderInWorldEntity> alreadyFilterList = BuilderInWorldUtils.FilterEntitiesBySmartItemComponentAndActions(entitiesList);
        if(alreadyFilterList.Count > 0)
            actionsListView.AddActionEventAdapter(entitiesList);
    }

}
