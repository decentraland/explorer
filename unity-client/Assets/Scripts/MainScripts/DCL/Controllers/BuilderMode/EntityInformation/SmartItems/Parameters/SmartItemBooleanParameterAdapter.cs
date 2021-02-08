using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemBooleanParameterAdapter : SmartItemUIParameterAdapter
{
    public Toggle boolParameterToggle;

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        boolParameterToggle.gameObject.SetActive(true);

        bool defaultParameter = false;
        bool.TryParse(parameter.@default, out defaultParameter);

        boolParameterToggle.isOn = defaultParameter;
    }
}
