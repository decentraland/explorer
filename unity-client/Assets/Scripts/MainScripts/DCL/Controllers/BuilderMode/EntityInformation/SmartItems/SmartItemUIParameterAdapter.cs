using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemUIParameterAdapter : MonoBehaviour
{

    public TextMeshProUGUI labelTxt;

    public virtual void SetEntityList(List<DCLBuilderInWorldEntity> entityList)
    {

    }

    public virtual void SetParameter(SmartItemParameter parameter)
    {
        labelTxt.text = parameter.label;

        switch (parameter.type)
        {
            case "boolean":
              

                break;
            case "text":
       
                break;
            case "textarea":

                break;
            case "float":

                break;
            case "integer":
        
                break;
            case "slider":
                break;
            case "options":
                break;
            case "entity":
                break;
            case "actions":
                break;

        }

    }
}
