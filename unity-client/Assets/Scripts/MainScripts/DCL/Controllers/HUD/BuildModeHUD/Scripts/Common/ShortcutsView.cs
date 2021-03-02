using System;
using UnityEngine;
using UnityEngine.UI;

public interface IShortcutsView
{
    event Action OnCloseButtonClick;

    void OnCloseClick();
    void SetActive(bool isActive);
}

public class ShortcutsView : MonoBehaviour, IShortcutsView
{
    public event System.Action OnCloseButtonClick;

    [SerializeField] internal Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseClick);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void OnCloseClick()
    {
        OnCloseButtonClick?.Invoke();
    }
}
