using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserContextConfirmationDialog : MonoBehaviour, IConfirmationDialog
{
    public TextMeshProUGUI dialogText;
    public Button cancelButton;
    public Button confirmButton;

    public void Awake()
    {

    }

    public void SetText(string text)
    {
        dialogText.text = text;
    }

    public void Show(System.Action onConfirm = null, System.Action onCancel = null)
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => { onConfirm?.Invoke(); Hide(); });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => { onCancel?.Invoke(); Hide(); });

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
