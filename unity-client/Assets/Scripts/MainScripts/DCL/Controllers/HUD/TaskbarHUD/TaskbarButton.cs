using UnityEngine;
using UnityEngine.UI;

public class TaskbarButton : MonoBehaviour
{
    public Button openButton;
    public GameObject lineOffIndicator;
    public GameObject lineOnIndicator;

    public event System.Action<TaskbarButton> OnOpen;
    public void Initialize()
    {
        openButton.onClick.AddListener(OnOpenButtonClick);
    }

    private void OnOpenButtonClick()
    {
        OnOpen?.Invoke(this);
    }

    public void SetLineIndicator(bool on)
    {
        lineOnIndicator?.SetActive(on);
        lineOffIndicator?.SetActive(!on);
    }
}
