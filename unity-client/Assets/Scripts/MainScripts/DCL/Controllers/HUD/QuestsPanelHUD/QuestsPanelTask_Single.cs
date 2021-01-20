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

        public void Populate(string newPayload)
        {
            payload = JsonUtility.FromJson<TaskPayload_Single>(newPayload);
            taskName.text = payload.name;
            status.isOn = payload.isDone;
        }
    }
}