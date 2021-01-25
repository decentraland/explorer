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

        private QuestModel quest;

        internal Action readMoreDelegate;

        private void Awake()
        {
            readMoreButton.onClick.AddListener(() => readMoreDelegate?.Invoke());
            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            DataStore.Quests.pinnedQuests.OnAdded += OnPinnedQuests;
            DataStore.Quests.pinnedQuests.OnRemoved += OnUnpinnedQuest;
        }
        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;

            readMoreDelegate = () => OnReadMoreClicked?.Invoke(quest.id);
            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_entry);
            pinQuestToggle.SetIsOnWithoutNotify(DataStore.Quests.pinnedQuests.Contains(quest.id));
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
            DataStore.Quests.pinnedQuests.OnAdded -= OnUnpinnedQuest;
            DataStore.Quests.pinnedQuests.OnRemoved -= OnPinnedQuests;
        }
    }
}