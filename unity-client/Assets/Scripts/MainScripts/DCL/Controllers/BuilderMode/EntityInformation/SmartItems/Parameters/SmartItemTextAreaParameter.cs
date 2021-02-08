using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemTextAreaParameter : SmartItemUIParameterAdapter
{
    public TMP_InputField textParameterInputField;

    private void Start()
    {
        textParameterInputField.onEndEdit.AddListener(OnValueChange);
    }

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        textParameterInputField.gameObject.SetActive(true);

        textParameterInputField.contentType = TMP_InputField.ContentType.Standard;
        textParameterInputField.text = (string)  parameter.@default;
    }

    public void OnValueChange(string text)
    {
     
    }
}