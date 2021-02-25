using UnityEngine;
using UnityEngine.UI;

public class ShortcutsView : MonoBehaviour
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
