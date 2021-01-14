using System;
using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestPanel
{
    public class QuestUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private RectTransform taskContainer;
        [SerializeField] private GameObject taskPrefab;

        public void Populate(QuestPanelModel quest)
        {
            CleanUpQuestsList(); //TODO Reuse already instantiated quests

            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail);
            for (int i = 0; i < quest.tasks.Length; i++)
            {
                CreateTask(quest.tasks[i]);
            }
        }

        internal void SetThumbnail(string thumbnailURL)
        {
            throw new NotImplementedException();
        }

        internal void CreateTask(QuestPanelTask task)
        {
            var taskEntry = Instantiate(taskPrefab, taskContainer).GetComponent<QuestTaskUIEntry>();
            taskEntry.Populate(task);
        }

        internal void CleanUpQuestsList()
        {
            while (taskContainer.childCount > 0)
            {
                Destroy(taskContainer.GetChild(0).gameObject);
            }
        }
    }
}