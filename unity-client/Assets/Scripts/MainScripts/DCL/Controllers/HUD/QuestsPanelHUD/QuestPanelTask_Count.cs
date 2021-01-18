using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestPanelTask_Count : MonoBehaviour, IQuestPanelTask
    {
        [Serializable]
        public class Model
        {
            public string name;
            public int start;
            public int end;
            public int current;
        }

        [SerializeField] private TextMeshProUGUI stepName;
        [SerializeField] private TextMeshProUGUI start;
        [SerializeField] private TextMeshProUGUI end;
        [SerializeField] private TextMeshProUGUI current;
        [SerializeField] private Image ongoingProgress;

        internal Model model;

        public void Populate(string payload)
        {
            model = JsonUtility.FromJson<Model>(payload);
            stepName.text = model.name;
            start.text = model.start.ToString();
            current.text = model.current.ToString();
            end.text = model.end.ToString();

            ongoingProgress.fillAmount = (float)model.current / model.end;
        }
    }
}