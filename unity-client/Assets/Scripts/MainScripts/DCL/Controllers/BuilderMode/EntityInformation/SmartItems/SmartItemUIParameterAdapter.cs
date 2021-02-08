using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemUIParameterAdapter : MonoBehaviour
{

    public TextMeshProUGUI labelTxt;
    public Action<SmartItemParameter> OnParameterChange;

    protected SmartItemParameter currentParameter;

    public virtual void SetParameter(SmartItemParameter parameter)
    {
        currentParameter = parameter;
        labelTxt.text = parameter.label;
    }

    public virtual void ChangeParameter()
    {
        OnParameterChange?.Invoke(currentParameter);
    }
}
