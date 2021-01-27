using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelTask_Count : MonoBehaviour, IQuestsPanelTask
    {
        [SerializeField] private TextMeshProUGUI stepName;
        [SerializeField] private TextMeshProUGUI start;
        [SerializeField] private TextMeshProUGUI end;
        [SerializeField] private TextMeshProUGUI current;
        [SerializeField] private Image ongoingProgress;

        internal TaskPayload_Count payload;

        public void Populate(QuestTask task)
        {
            payload = JsonUtility.FromJson<TaskPayload_Count>(task.payload);
            stepName.text = task.name;
            start.text = payload.start.ToString();
            current.text = payload.current.ToString();
            end.text = payload.end.ToString();

            ongoingProgress.fillAmount = (float)payload.current / payload.end;
        }
    }
}