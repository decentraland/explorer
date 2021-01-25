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
        [SerializeField] private Toggle pinQuestToggle;

        private QuestModel quest;
        private bool isExpanded;

        public void Awake()
        {
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);

            expandCollapseButton.gameObject.SetActive(false);
            SetExpandCollapseState(true);
            expandCollapseButton.onClick.AddListener(() => SetExpandCollapseState(!isExpanded));
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            questTitle.text = quest.name;
            SetThumbnail(quest.icon);
            QuestSection currentSection = quest.sections.First(x => x.progress < 1f);
            sectionTitle.text = $"{currentSection.name} - {currentSection.progress*100}%";
            progress.fillAmount = currentSection.progress;

            // TODO Reuse entries
            CleanUpTasksList();
            //TODO Distribute creation in frames to avoid hiccups
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

        internal void CleanUpTasksList()
        {
            for(int i = tasksContainer.childCount - 1; i >= 0; i--)
                Destroy(tasksContainer.GetChild(i).gameObject);
        }

        internal void SetExpandCollapseState(bool newIsExpanded)
        {
            isExpanded = newIsExpanded;
            expandIcon.SetActive(!isExpanded);
            collapseIcon.SetActive(isExpanded);
            tasksContainer.gameObject.SetActive(isExpanded);
            OnLayoutRebuildRequested?.Invoke();
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (isOn)
            {
                if (!DataStore.Quests.pinnedQuests.Contains(quest.id))
                    DataStore.Quests.pinnedQuests.Add(quest.id);
            }
            else
            {
                if (DataStore.Quests.pinnedQuests.Contains(quest.id))
                    DataStore.Quests.pinnedQuests.Remove(quest.id);
            }
        }

        public void SetPinStatus(bool isPinned)
        {
            pinQuestToggle.SetIsOnWithoutNotify(isPinned);
        }
    }
}
