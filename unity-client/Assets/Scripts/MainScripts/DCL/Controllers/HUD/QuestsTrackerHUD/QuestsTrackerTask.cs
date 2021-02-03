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
            jumpInButton.gameObject.SetActive(!string.IsNullOrEmpty(task.coordinates));
            jumpInDelegate = () => WebInterface.SendChatMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.NONE,
                recipient = string.Empty,
                body = $"/goto {task.coordinates}",
            });

            switch (task.type)
            {
                case "single":
                    ApplyPayload(JsonUtility.FromJson<TaskPayload_Single>(task.payload));
                    break;
                case "count":
                    ApplyPayload(JsonUtility.FromJson<TaskPayload_Count>(task.payload));
                    break;
            }
        }

        internal void ApplyPayload(TaskPayload_Single taskPayload)
        {
            progress.fillAmount = taskPayload.Progress();
            progressText.text = $"{taskPayload.Progress().ToString()}/1";
        }

        internal void ApplyPayload(TaskPayload_Count taskPayload)
        {
            progress.fillAmount = taskPayload.Progress();
            progressText.text = $"{taskPayload.current}/{taskPayload.end}";
        }
    }
}