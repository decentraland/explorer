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

        public void Populate(QuestTask task)
        {
            switch (task.type)
            {
                case "single":
                    ApplyPayload(task.name, JsonUtility.FromJson<TaskPayload_Single>(task.payload));
                    break;
                case "count":
                    ApplyPayload(task.name, JsonUtility.FromJson<TaskPayload_Count>(task.payload));
                    break;
            }
        }

        internal void ApplyPayload(string taskName, TaskPayload_Single taskPayload)
        {
            taskTitle.text = taskName;
            progress.fillAmount = taskPayload.Progress();
            progressText.text = $"{taskPayload.Progress().ToString()}/1";
        }

        internal void ApplyPayload(string taskName, TaskPayload_Count taskPayload)
        {
            taskTitle.text = taskName;
            progress.fillAmount = taskPayload.Progress();
            progressText.text = $"{taskPayload.current}/{taskPayload.end}";
        }
    }
}