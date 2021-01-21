using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// It represents a Portable Experience item in the taskbar.
/// </summary>
public class PortableExperienceTaskbarItem : MonoBehaviour
{
    [SerializeField]
    private TaskbarButton button;

    [SerializeField]
    private TextMeshProUGUI tooltipText;

    [SerializeField]
    private Image icon;

    public TaskbarButton mainButton { get => button; }

    internal void ConfigureItem(string peName)
    {
        tooltipText.text = peName;
        button.Initialize();
    }

    internal void ShowPortableExperienceMenu(bool visible)
    {

    }
}
