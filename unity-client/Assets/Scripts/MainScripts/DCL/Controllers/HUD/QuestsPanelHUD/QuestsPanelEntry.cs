using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelEntry : MonoBehaviour
    {
        public event Action<string> OnReadMoreClicked;

        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Button readMoreButton;
        [SerializeField] private Toggle pinQuestToggle;
        [SerializeField] private Image progressInTitle;
        [SerializeField] private RectTransform completedProgressInTitle;
        [SerializeField] private RectTransform completedMarkInTitle;

        private QuestModel quest;

        internal Action readMoreDelegate;
        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;

        private void Awake()
        {
            readMoreButton.onClick.AddListener(() => readMoreDelegate?.Invoke());
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            pinnedQuests.OnAdded += OnPinnedQuests;
            pinnedQuests.OnRemoved += OnUnpinnedQuest;
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;

            readMoreDelegate = () => OnReadMoreClicked?.Invoke(quest.id);
            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_entry);
            pinQuestToggle.SetIsOnWithoutNotify(pinnedQuests.Contains(quest.id));

            var questCompleted = quest.isCompleted;
            pinQuestToggle.gameObject.SetActive(!questCompleted);
            progressInTitle.fillAmount = quest.progress;
            completedProgressInTitle.gameObject.SetActive(questCompleted);
            completedMarkInTitle.gameObject.SetActive(questCompleted);
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (quest.isCompleted)
            {
                pinnedQuests.Remove(quest.id);
                pinQuestToggle.SetIsOnWithoutNotify(false);
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

        private void OnPinnedQuests(string questId)
        {
            if (quest != null && quest.id == questId)
                pinQuestToggle.SetIsOnWithoutNotify(true);
        }

        private void OnUnpinnedQuest(string questId)
        {
            if (quest != null && quest.id == questId)
                pinQuestToggle.SetIsOnWithoutNotify(false);
        }

        internal void SetThumbnail(string thumbnailURL)
        {

        }

        private void OnDestroy()
        {
            pinnedQuests.OnAdded -= OnUnpinnedQuest;
            pinnedQuests.OnRemoved -= OnPinnedQuests;
        }
    }
}