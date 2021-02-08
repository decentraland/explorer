using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemFloatParameter : SmartItemUIParameterAdapter
{
    public TMP_InputField textParameterInputField;

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        textParameterInputField.gameObject.SetActive(true);
        
        textParameterInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        textParameterInputField.text = (string) parameter.@default;
    }
}
