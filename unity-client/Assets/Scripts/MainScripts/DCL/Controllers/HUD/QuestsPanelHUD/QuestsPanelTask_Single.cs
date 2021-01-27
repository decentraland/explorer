using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelTask_Single : MonoBehaviour, IQuestsPanelTask
    {
        [SerializeField] private TextMeshProUGUI taskName;
        [SerializeField] private Toggle status;

        internal TaskPayload_Single payload;

        public void Populate(QuestTask task)
        {
            payload = JsonUtility.FromJson<TaskPayload_Single>(task.payload);
            taskName.text = task.name;
            status.isOn = payload.isDone;
        }
    }
}