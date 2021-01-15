using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestPanel
{
    public class QuestTaskUIEntry_Single : MonoBehaviour, IQuestTaskUIEntry
    {
        [Serializable]
        public class Model
        {
            public string name;
            public bool isDone;
        }

        [SerializeField] private TextMeshProUGUI taskName;
        [SerializeField] private Toggle status;

        internal Model model;

        public void Populate(string payload)
        {
            model = JsonUtility.FromJson<Model>(payload);
            taskName.text = model.name;
            status.isOn = model.isDone;
        }
    }
}