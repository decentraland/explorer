using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private RectTransform sectionsContainer;
        [SerializeField] private GameObject sectionPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Toggle pinQuestToggle;

        private QuestModel quest;
        private static BaseCollection<string> baseCollection => DataStore.Quests.pinnedQuests;

        private void Awake()
        {
            closeButton.onClick.AddListener(Close);

            pinQuestToggle.onValueChanged.AddListener(OnPinToggleValueChanged);
            baseCollection.OnAdded += OnPinnedQuests;
            baseCollection.OnRemoved += OnUnpinnedQuest;
        }

        public void Populate(QuestModel newQuest)
        {
            quest = newQuest;
            CleanUpQuestsList(); //TODO Reuse already instantiated quests

            questName.text = quest.name;
            description.text = quest.description;
            SetThumbnail(quest.thumbnail_banner);
            for (int i = 0; i < quest.sections.Length; i++)
            {
                CreateTask(quest.sections[i]);
            }
            Utils.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            pinQuestToggle.SetIsOnWithoutNotify(baseCollection.Contains(quest.id));
        }

        private void OnPinToggleValueChanged(bool isOn)
        {
            if (quest == null)
                return;

            if (isOn)
            {
                if (!baseCollection.Contains(quest.id))
                    baseCollection.Add(quest.id);
            }
            else
            {
                if (baseCollection.Contains(quest.id))
                    baseCollection.Remove(quest.id);
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

        internal void CreateTask(QuestSection section)
        {
            var taskEntry = Instantiate(sectionPrefab, sectionsContainer).GetComponent<QuestsPanelSection>();
            taskEntry.Populate(section);
        }

        internal void CleanUpQuestsList()
        {
            for(int i = sectionsContainer.childCount - 1; i >= 0; i--)
                Destroy(sectionsContainer.GetChild(i).gameObject);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}