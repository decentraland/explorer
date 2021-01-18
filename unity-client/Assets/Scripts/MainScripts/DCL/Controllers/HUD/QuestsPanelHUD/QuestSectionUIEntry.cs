using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestPanel
{
    public class QuestSectionUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI taskName;
        [SerializeField] private RectTransform tasksContainer;
        [SerializeField] private QuestTaskUIFactory factory;

        public void Populate(QuestSection section)
        {
            CleanUpTasksList(); //TODO: Reuse already instantiated steps
            taskName.text = section.name;
            foreach (QuestTask task in section.tasks)
            {
                CreateTask(task);
            }
        }

        internal void CreateTask(QuestTask task)
        {
            GameObject prefab = factory.GetPrefab(task.type);
            if (prefab == null)
            {
                Debug.LogError($"Type: {task.type} was not found in QuestTaskFactory");
                return;
            }

            var taskUIEntry = Instantiate(prefab, tasksContainer).GetComponent<IQuestTaskUIEntry>();
            taskUIEntry.Populate(task.payload);
        }

        internal void CleanUpTasksList()
        {
            for(int i = tasksContainer.childCount - 1; i >= 0; i--)
                Destroy(tasksContainer.GetChild(i).gameObject);
        }
    }
}