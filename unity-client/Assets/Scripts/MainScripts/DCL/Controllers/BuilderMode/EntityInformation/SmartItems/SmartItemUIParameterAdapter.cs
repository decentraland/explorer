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
    }
}
