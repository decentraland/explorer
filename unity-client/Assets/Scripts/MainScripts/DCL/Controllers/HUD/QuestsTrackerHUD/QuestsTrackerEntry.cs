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
        [SerializeField] private RawImage iconImage;

        private AssetPromise_Texture iconPromise;

        private QuestModel quest;
        private bool isExpanded;
        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;

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
            SetIcon(quest.icon);
            QuestSection currentSection = quest.sections.First(x => x.progress < 1f);
            sectionTitle.text = $"{currentSection.name} - {currentSection.progress * 100}%";
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

        internal void SetIcon(string iconURL)
        {
            if (iconPromise != null)
            {
                iconPromise.ClearEvents();
                AssetPromiseKeeper_Texture.i.Forget(iconPromise);
            }

            if (string.IsNullOrEmpty(iconURL))
                return;

            iconPromise = new AssetPromise_Texture(iconURL);
            iconPromise.OnSuccessEvent += OnIconReady;
            iconPromise.OnFailEvent += x => { Debug.Log($"Error downloading quest tracker entry icon: {iconURL}"); };

            AssetPromiseKeeper_Texture.i.Keep(iconPromise);
        }

        private void OnIconReady(Asset_Texture assetTexture)
        {
            iconImage.texture = assetTexture.texture;
        }

        internal void CleanUpTasksList()
        {
            for (int i = tasksContainer.childCount - 1; i >= 0; i--)
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

            if (quest.isCompleted)
            {
                pinnedQuests.Remove(quest.id);
                SetPinStatus(false);
                return;
            }

            if (isOn)
            {
                if (!pinnedQuests.Contains(quest.id))
                    pinnedQuests.Add(quest.id);
            }
            else
            {
                pinnedQuests.Remove(quest.id);
            }
        }

        public void SetPinStatus(bool isPinned)
        {
            pinQuestToggle.SetIsOnWithoutNotify(isPinned);
        }
    }
}