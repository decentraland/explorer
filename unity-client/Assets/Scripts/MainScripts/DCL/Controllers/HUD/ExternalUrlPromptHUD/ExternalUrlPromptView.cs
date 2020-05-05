using UnityEngine;
using UnityEngine.UI;

public class ExternalUrlPromptView : MonoBehaviour
{
    [SerializeField] GameObject content;
    [SerializeField] Button closeButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button cancelButton;
    [SerializeField] TMPro.TextMeshProUGUI domainText;
    [SerializeField] TMPro.TextMeshProUGUI urlText;
    [SerializeField] Toggle trustToggle;
}
