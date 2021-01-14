using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestPanel
{
    public class QuestStepUIEntry_Single : MonoBehaviour, IQuestStepUIEntry
    {
        [Serializable]
        public class Model
        {
            public string name;
            public bool isDone;
        }

        [SerializeField] private TextMeshProUGUI stepName;
        [SerializeField] private Toggle status;

        internal Model model;

        public void Populate(string payload)
        {
            model = JsonUtility.FromJson<Model>(payload);
            stepName.text = model.name;
            status.isOn = model.isDone;
        }
    }
}