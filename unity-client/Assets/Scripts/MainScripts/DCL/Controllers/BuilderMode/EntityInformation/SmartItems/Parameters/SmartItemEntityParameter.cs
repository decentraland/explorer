using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemEntityParameter : SmartItemUIParameterAdapter
{
    public TMP_Dropdown dropDown;

    const string parameterType = "entity";

    List<DCLBuilderInWorldEntity> entitiesList;

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

        GenerateDropdownContent();


    }


    void GenerateDropdownContent()
    {
        dropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        foreach (DCLBuilderInWorldEntity entity in entitiesList)
        {
            optionsLabelList.Add(entity.GetDescriptiveName());
        }

        dropDown.AddOptions(optionsLabelList);
    }
}
