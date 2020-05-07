using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
    public class Model
    {
        public NotificationFactory.Type type;
        public string message;
        public string buttonMessage;
        public float timer;
        public string scene;
        public System.Action callback;
        public string externalCallbackID;
        public string groupID;
    }

    [SerializeField]
    internal TextMeshProUGUI messageLabel;

    [SerializeField]
    private Button actionButton;

    [SerializeField]
    private TextMeshProUGUI actionButtonLabel;

    public Notification.Model model { get; private set; } = new Model();

    public event System.Action<Notification> OnNotificationDismissed;

    private void OnEnable()
    {
        actionButton.onClick.AddListener(Dismiss);
    }

    private void OnDisable()
    {
        actionButton.onClick.RemoveAllListeners();
    }

    public void Initialize(Notification.Model model)
    {
        this.model = model;

        if (!string.IsNullOrEmpty(this.model.message))
        {
            messageLabel.text = this.model.message;
        }

        if (!string.IsNullOrEmpty(this.model.buttonMessage))
        {
            actionButtonLabel.text = this.model.buttonMessage;
        }

        if (this.model.timer > 0)
        {
            StartCoroutine(TimerCoroutine(this.model.timer));
        }

        if (!string.IsNullOrEmpty(this.model.scene))
        {
            string sceneID = CommonScriptableObjects.sceneID ?? string.Empty;
            CurrentSceneUpdated(sceneID, string.Empty);
        }

        if (this.model.callback != null)
        {
            actionButton.onClick.AddListener(this.model.callback.Invoke);
        }

        if (!string.IsNullOrEmpty(this.model.externalCallbackID))
        {
            actionButton.onClick.AddListener(() =>
            {
                // TODO: send message to kernel with callbackID
            });
        }
    }

    private IEnumerator TimerCoroutine(float timer)
    {
        yield return WaitForSecondsCache.Get(timer);
        Dismiss();
    }

    private void CurrentSceneUpdated(string current, string previous)
    {
        if (string.CompareOrdinal(current, model.scene) != 0)
        {
            Dismiss();
        }
    }

    public void Dismiss()
    {
        StopAllCoroutines();
        OnNotificationDismissed?.Invoke(this);
    }
}
