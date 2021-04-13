using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IPublishPopupView
{
    public float currentProgress { get; }

    void PublishStart();
    void PublishEnd(string message);
    void SetPercentage(float newValue);
}

public class PublishPopupView : MonoBehaviour, IPublishPopupView
{
    public float currentProgress => loadingBar.currentPercentage;

    [SerializeField] internal TMP_Text resultText;
    [SerializeField] internal LoadingBar loadingBar;
    [SerializeField] internal Button closeButton;

    private const string VIEW_PATH = "Common/PublishPopupView";

    internal static PublishPopupView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PublishPopupView>();
        view.gameObject.name = "_PublishPopupView";

        return view;
    }

    private void Awake() { closeButton.onClick.AddListener(CloseModal); }

    private void OnDestroy() { closeButton.onClick.RemoveListener(CloseModal); }

    public void PublishStart()
    {
        gameObject.SetActive(true);
        loadingBar.SetActive(true);
        resultText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public void PublishEnd(string message)
    {
        loadingBar.SetActive(false);
        resultText.text = message;
        resultText.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }

    private void CloseModal() { gameObject.SetActive(false); }
}