using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questTitle;
        [SerializeField] private RawImage questIcon;
        [SerializeField] private TextMeshProUGUI sectionTitle;
        [SerializeField] private Image progress;
        [SerializeField] private RectTransform tasksContainer;
        [SerializeField] private GameObject taskPrefab;

        public void Populate(QuestModel quest)
        {
            questTitle.text = quest.name;
            SetThumbnail(quest.icon);
            QuestSection currentSection = quest.sections.First(x => x.progress < 1f);
            sectionTitle.text = $"{currentSection.name} - {currentSection.progress*100}%";
            progress.fillAmount = currentSection.progress;
            foreach (QuestTask task in currentSection.tasks)
            {
                CreateTask(task);
            }
        }

        internal void CreateTask(QuestTask task)
        {
            var taskUIEntry = Instantiate(taskPrefab, tasksContainer).GetComponent<QuestsTrackerTask>();
            taskUIEntry.Populate(task);
        }

        internal void SetThumbnail(string url)
        {

        }
    }
}
