using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TaskbarButton : MonoBehaviour
{
    [FormerlySerializedAs("openButton")]
    public Button toggleButton;
    public GameObject lineOffIndicator;
    public GameObject lineOnIndicator;

    public event System.Action<TaskbarButton> OnToggleOn;
    public event System.Action<TaskbarButton> OnToggleOff;

    public bool toggledOn { get; private set; } = true;

    public void Initialize()
    {
        toggleButton.onClick.AddListener(OnToggleButtonClick);
        SetToggleState(false, useCallback: false);
    }

    private void OnToggleButtonClick()
    {
        SetToggleState(!toggledOn);
    }

    public void SetToggleState(bool on, bool useCallback = true)
    {
        if (toggledOn == on)
            return;

        SetLineIndicator(on);

        if (useCallback)
        {
            if (on)
                OnToggleOn?.Invoke(this);
            else
                OnToggleOff?.Invoke(this);
        }

        toggledOn = on;
    }

    public void SetLineIndicator(bool on)
    {
        if (lineOnIndicator != null)
            lineOnIndicator.SetActive(on);

        if (lineOffIndicator != null)
            lineOffIndicator.SetActive(!on);

        toggledOn = on;
    }
}
