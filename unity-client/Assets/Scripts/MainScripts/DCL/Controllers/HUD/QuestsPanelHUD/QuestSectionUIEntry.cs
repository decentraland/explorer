using TMPro;
using UnityEngine;

namespace DCL.Huds.QuestPanel
{
    public class QuestSectionUIEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI taskName;
        [SerializeField] private RectTransform tasksContainer;
        [SerializeField] private QuestTaskUIFactory factory;

        public void Populate(QuestPanelSection section)
        {
            CleanUpTasksList(); //TODO: Reuse already instantiated steps
            taskName.text = section.name;
            foreach (QuestPanelTask task in section.tasks)
            {
                CreateTask(task);
            }
        }

        internal void CreateTask(QuestPanelTask task)
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
            while (tasksContainer.childCount > 0)
            {
                Destroy(tasksContainer.GetChild(0).gameObject);
            }
        }
    }
}