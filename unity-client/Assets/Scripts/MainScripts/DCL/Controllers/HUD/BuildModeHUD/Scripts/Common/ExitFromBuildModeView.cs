using System;
using UnityEngine;
using UnityEngine.UI;

public interface IExitFromBuildModeView
{
    event Action OnCancelExit;
    event Action OnConfirmExit;

    void SetActive(bool isActive);
    void CancelExit();
    void ConfirmExit();
}

public class ExitFromBuildModeView : MonoBehaviour, IExitFromBuildModeView
{
    public event Action OnCancelExit;
    public event Action OnConfirmExit;

    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button confirmButton;

    private const string VIEW_PATH = "Common/ExitFromBuildModeView";

    internal static ExitFromBuildModeView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ExitFromBuildModeView>();
        view.gameObject.name = "_ExitFromBiWModalView";

        return view;
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(CancelExit);
        cancelButton.onClick.AddListener(CancelExit);
        confirmButton.onClick.AddListener(ConfirmExit);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(CancelExit);
        cancelButton.onClick.RemoveListener(CancelExit);
        confirmButton.onClick.RemoveListener(ConfirmExit);
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void CancelExit() { OnCancelExit?.Invoke(); }

    public void ConfirmExit() { OnConfirmExit?.Invoke(); }
}