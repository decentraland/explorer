using System;
using TMPro;
using UnityEngine;

internal class SortDropdownButton : MonoBehaviour
{
    public static event Action<string> OnClick;

    [SerializeField] private Button_OnPointerDown button;
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        button.onPointerDown += () => OnClick?.Invoke(label.text);
    }

    public void SetText(string text)
    {
        label.text = text;
    }
}
