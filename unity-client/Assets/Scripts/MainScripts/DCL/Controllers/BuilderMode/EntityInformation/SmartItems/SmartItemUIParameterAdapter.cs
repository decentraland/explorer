using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemUIParameterAdapter : MonoBehaviour
{

    public TextMeshProUGUI labelTxt;

    //Types
    public Toggle boolParameterToggle;
    public TMP_InputField textParameterInputField;
    public Slider sliderParameter;
    public TMP_Dropdown dropdownParameter;


    public void SetParameter(SmartItemParameter parameter)
    {
        labelTxt.text = parameter.label;

        switch (parameter.type)
        {
            case "boolean":
                boolParameterToggle.gameObject.SetActive(true);
                bool defaultParameter = false;
                bool.TryParse(parameter.defaultValue, out defaultParameter);
                boolParameterToggle.isOn = defaultParameter;

                break;
            case "text":
                textParameterInputField.gameObject.SetActive(true);
                textParameterInputField.text = parameter.defaultValue;
                break;
            case "textarea":
                textParameterInputField.gameObject.SetActive(true);
                textParameterInputField.text = parameter.defaultValue;
                break;
            case "float":
                textParameterInputField.gameObject.SetActive(true);
                textParameterInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                textParameterInputField.text = parameter.defaultValue;
                break;
            case "integer":
                textParameterInputField.gameObject.SetActive(true);
                textParameterInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                textParameterInputField.text = parameter.defaultValue;
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
