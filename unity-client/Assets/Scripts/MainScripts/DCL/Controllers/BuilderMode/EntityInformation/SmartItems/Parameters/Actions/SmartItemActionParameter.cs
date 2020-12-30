using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemActionParameter : SmartItemUIParameterAdapter
{
    public ActionsListView actionsListView;
    public Button addActionBtn;

    const string parameterType = "actions";

    List<DCLBuilderInWorldEntity> entitiesList;

    private void Start()
    {
        addActionBtn.onClick.AddListener(AddEventAction);
    }

    public override void SetEntityList(List<DCLBuilderInWorldEntity> entitiesList)
    {
        base.SetEntityList(entitiesList);

        this.entitiesList = entitiesList;
    }


    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        if (parameter.type != parameterType)
            return;
    }


    public void AddEventAction()
    {
        List<DCLBuilderInWorldEntity> alreadyFilterList = BuilderInWorldUtils.FilterEntitiesBySmartItemComponentAndActions(entitiesList);
        if(alreadyFilterList.Count > 0)
            actionsListView.AddActionEventAdapter(entitiesList);
    }

}
