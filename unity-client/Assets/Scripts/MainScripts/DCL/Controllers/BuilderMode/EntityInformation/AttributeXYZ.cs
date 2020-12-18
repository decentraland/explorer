using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttributeXYZ : MonoBehaviour
{
    public TMP_InputField xField;
    public TMP_InputField yField;
    public TMP_InputField zField;

    public event Action<float> OnXChanged;
    public event Action<float> OnYChanged;
    public event Action<float> OnZChanged;

    
    public void SetValues(Vector3 value)
    {
        xField.text = value.x.ToString("0.##");
        yField.text = value.y.ToString("0.##");
        zField.text = value.z.ToString("0.##");
    }

    public void ChangeXValue(string value)
    {
        OnXChanged?.Invoke(float.Parse(value));
    }

    public void ChangeYValue(string value)
    {
        OnYChanged?.Invoke(float.Parse(value));
    }

    public void ChangeZValue(string value)
    {
        OnZChanged?.Invoke(float.Parse(value));
    }
}
