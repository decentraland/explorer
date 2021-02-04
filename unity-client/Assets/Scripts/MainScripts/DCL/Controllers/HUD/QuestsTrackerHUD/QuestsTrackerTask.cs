using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerTask : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI taskTitle;
        [SerializeField] private Image progress;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button jumpInButton;

        private Action jumpInDelegate;

        public void Awake()
        {
            jumpInButton.onClick.AddListener(() => { jumpInDelegate?.Invoke();});
        }

        public void Populate(QuestTask task)
        {
            taskTitle.text = task.name;
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });

            jumpInButton.gameObject.SetActive(task.progress < 1 && !string.IsNullOrEmpty(task.coordinates));
            progress.fillAmount = task.progress;
            switch (task.type)
            {
                case "single":
                    SetProgressText(task.progress, 1);
                    break;
                case "count":
                    var payload = JsonUtility.FromJson<TaskPayload_Count>(task.payload);
                    SetProgressText(payload.current, payload.end);
                    break;
            }
        }

        internal void SetProgressText(float current, float end)
        {
            progressText.text = $"{current}/{end}";
        }
    }
}