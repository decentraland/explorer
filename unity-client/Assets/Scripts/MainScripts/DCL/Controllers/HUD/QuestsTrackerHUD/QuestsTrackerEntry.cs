using DCL.Helpers;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerEntry : MonoBehaviour
    {
        public event Action OnLayoutRebuildRequested;

        [SerializeField] private TextMeshProUGUI questTitle;
        [SerializeField] private RawImage questIcon;
        [SerializeField] private TextMeshProUGUI sectionTitle;
        [SerializeField] private Image progress;
        [SerializeField] private RectTransform tasksContainer;
        [SerializeField] private GameObject taskPrefab;
        [SerializeField] private Button expandCollapseButton;
        [SerializeField] private GameObject expandIcon;
        [SerializeField] private GameObject collapseIcon;

        private bool isExpanded;

        public void Awake()
        {
            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
        }

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
            expandCollapseButton.gameObject.SetActive(currentSection.tasks.Length > 0);
            SetExpandCollapseState(true);
        }

        internal void CreateTask(QuestTask task)
        {
            var taskUIEntry = Instantiate(taskPrefab, tasksContainer).GetComponent<QuestsTrackerTask>();
            taskUIEntry.Populate(task);
        }

        internal void SetThumbnail(string url)
        {

        }

        internal void SetExpandCollapseState(bool newIsExpanded)
        {
            isExpanded = newIsExpanded;
            expandIcon.SetActive(isExpanded);
            collapseIcon.SetActive(!isExpanded);
            tasksContainer.gameObject.SetActive(isExpanded);
            OnLayoutRebuildRequested?.Invoke();
        }
    }
}
