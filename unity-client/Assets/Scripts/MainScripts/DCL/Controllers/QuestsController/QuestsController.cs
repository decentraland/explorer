using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCL.QuestsController
{
    public delegate void QuestProgressed(string questId);
    public delegate void QuestCompleted(string questId);
    public delegate void SectionCompleted(string questId, string sectionId);
    public delegate void SectionUnlocked(string questId, string sectionId);
    public delegate void TaskProgressed(string questId, string sectionId, string taskId);

    public interface IQuestsController
    {
        event QuestProgressed OnQuestProgressed;
        event QuestCompleted OnQuestCompleted;
        event SectionCompleted OnSectionCompleted;
        event SectionUnlocked OnSectionUnlocked;
        event TaskProgressed OnTaskProgressed;
    }

    public class QuestsController : MonoBehaviour, IQuestsController
    {
        private const string PINNED_QUESTS_KEY = "PinnedQuests";

        public static QuestsController i { get; private set; }

        public event QuestProgressed OnQuestProgressed;
        public event QuestCompleted OnQuestCompleted;
        public event SectionCompleted OnSectionCompleted;
        public event SectionUnlocked OnSectionUnlocked;
        public event TaskProgressed OnTaskProgressed;

        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;
        private static BaseDictionary<string, QuestModel> quests => DataStore.Quests.quests;

        private void Awake()
        {
            i = this;
            var savedPinnedQuests = PlayerPrefs.GetString(PINNED_QUESTS_KEY, null);
            if (!string.IsNullOrEmpty(savedPinnedQuests))
            {
                pinnedQuests.Set(Utils.ParseJsonArray<string[]>(savedPinnedQuests));
            }
            pinnedQuests.OnAdded += SavePinnedQuests;
            pinnedQuests.OnRemoved += SavePinnedQuests;
        }

        /// <summary>
        /// Bulk initialization of quests
        /// </summary>
        /// <param name="jsonMessage">it must contain a QuestModel array</param>
        public void InitializeQuests(string jsonMessage)
        {
            var parsedQuests = Utils.ParseJsonArray<List<QuestModel>>(jsonMessage);

            var completedQuestsIds = parsedQuests.Where(x => x.isCompleted).Select(x => x.id);
            foreach (string questId in completedQuestsIds)
            {
                pinnedQuests.Remove(questId);
            }

            quests.Set(parsedQuests.Select(x => (x.id, x)));
        }

        /// <summary>
        /// Update progress in a quest
        /// </summary>
        /// <param name="jsonMessage">it must contain a QuestModel</param>
        public void UpdateQuestProgress(string jsonMessage)
        {
            var progressedQuest = JsonUtility.FromJson<QuestModel>(jsonMessage);

            if (progressedQuest.isCompleted)
                pinnedQuests.Remove(progressedQuest.id);

            //Alex: Edge case. Progressed quest was not included in the initialization.
            // We invoke quests events but no sections ones.
            if (!quests.TryGetValue(progressedQuest.id, out QuestModel oldQuest))
            {
                quests.Add(progressedQuest.id, progressedQuest);
                OnQuestProgressed?.Invoke(progressedQuest.id);

                if (progressedQuest.isCompleted)
                    OnQuestCompleted?.Invoke(progressedQuest.id);
                return;
            }

            quests[progressedQuest.id] = progressedQuest;
            OnQuestProgressed?.Invoke(progressedQuest.id);

            for (int i = 0; i < progressedQuest.sections.Length; i++)
            {
                QuestSection newQuestSection = progressedQuest.sections[i];
                QuestSection nextQuestSection = (i + 1) < progressedQuest.sections.Length ? (progressedQuest.sections[i + 1]) : null;

                //Alex: Edge case. New quest reported contains a section that was previously not contained.
                // if it's completed, we call the SectionCompleted event and unlock the next one
                bool sectionCompleted = !oldQuest.TryGetSection(newQuestSection.id, out QuestSection oldQuestSection);

                sectionCompleted = sectionCompleted || Math.Abs(oldQuestSection.progress - newQuestSection.progress) > Mathf.Epsilon && newQuestSection.progress >= 1;

                if (sectionCompleted)
                {
                    OnSectionCompleted?.Invoke(progressedQuest.id, newQuestSection.id);
                    if (nextQuestSection != null)
                        OnSectionUnlocked?.Invoke(progressedQuest.id, nextQuestSection.id);
                }
            }

            if (!oldQuest.isCompleted && progressedQuest.isCompleted)
                OnQuestCompleted?.Invoke(progressedQuest.id);
        }

        private void SavePinnedQuests(string questId)
        {
            //TODO Alex: only save this once per frame
            PlayerPrefs.SetString(PINNED_QUESTS_KEY, JsonConvert.SerializeObject(pinnedQuests.Get()));
        }

        private void OnDestroy()
        {
            pinnedQuests.OnAdded -= SavePinnedQuests;
            pinnedQuests.OnRemoved -= SavePinnedQuests;
        }

        #region just for testing, dont merge this code. (IF you see this in a review, hit me)
        [SerializeField] private TextAsset questsJson;
        private void Start()
        {
            InitializeQuests(questsJson.text);
        }

        [ContextMenu("Add Entry")]
        private void UtilAddEntry()
        {
            var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
            OnQuestProgressed?.Invoke(quests[Random.Range(0, quests.Count)].id);
        }

        [ContextMenu("Section Completed")]
        private void CallSectionCompleted()
        {
            var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
            var quest = quests[Random.Range(0, quests.Count)];
            OnSectionCompleted?.Invoke(quest.id, quest.sections[Random.Range(0, quest.sections.Length)].id);
        }

        [ContextMenu("Section Unlocked")]
        private void CallSectionUnlocked()
        {
            var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
            var quest = quests[Random.Range(0, quests.Count)];
            OnSectionUnlocked?.Invoke(quest.id, quest.sections[Random.Range(0, quest.sections.Length)].id);
        }

        [ContextMenu("Complete pinned quest")]
        private void CompleteRandomPinnedQuest()
        {
            var pinnedQuest = pinnedQuests[Random.Range(0, pinnedQuests.Count())];
            var quest = JsonUtility.FromJson<QuestModel>(JsonUtility.ToJson(quests[pinnedQuest]));
            foreach (QuestSection questSection in quest.sections)
            {
                questSection.progress = 1;
            }
            UpdateQuestProgress(JsonUtility.ToJson(quest));
        }
        #endregion
    }
}