using DCL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.QuestsController
{
    public delegate void QuestProgressed(string questId);
    public delegate void QuestCompleted(string questId);
    public delegate void SectionCompleted(string questId, string sectionId);
    public delegate void SectionUnlocked(string questId, string sectionId);
    public delegate void TaskProgressed(string questId, string sectionId, string taskId);
    public delegate void RewardObtained(string questId, string rewardId);

    public interface IQuestsController : IDisposable
    {
        event QuestProgressed OnQuestProgressed;
        event QuestCompleted OnQuestCompleted;
        event SectionCompleted OnSectionCompleted;
        event SectionUnlocked OnSectionUnlocked;
        event RewardObtained OnRewardObtained;

        void InitializeQuests(List<QuestModel> parsedQuests);
        void UpdateQuestProgress(QuestModel progressedQuest);
        void RemoveQuest(QuestModel quest);
    }

    public class QuestsController : IQuestsController
    {
        private const string PINNED_QUESTS_KEY = "PinnedQuests";

        public static IQuestsController i { get; internal set; }

        public event QuestProgressed OnQuestProgressed;
        public event QuestCompleted OnQuestCompleted;
        public event SectionCompleted OnSectionCompleted;
        public event SectionUnlocked OnSectionUnlocked;
        public event RewardObtained OnRewardObtained;

        private static BaseCollection<string> pinnedQuests => DataStore.i.Quests.pinnedQuests;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

        static QuestsController() { i = new QuestsController(); }

        public QuestsController()
        {
            var savedPinnedQuests = PlayerPrefs.GetString(PINNED_QUESTS_KEY, null);
            if (!string.IsNullOrEmpty(savedPinnedQuests))
            {
                pinnedQuests.Set(Utils.ParseJsonArray<string[]>(savedPinnedQuests));
            }
            pinnedQuests.OnAdded += OnPinnedQuestUpdated;
            pinnedQuests.OnRemoved += OnPinnedQuestUpdated;
        }

        /// <summary>
        /// Bulk initialization of quests
        /// </summary>
        /// <param name="parsedQuests"></param>
        public void InitializeQuests(List<QuestModel> parsedQuests)
        {
            var questsToUnpin = parsedQuests.Where(x => !x.canBePinned).Select(x => x.id);
            foreach (string questId in questsToUnpin)
            {
                pinnedQuests.Remove(questId);
            }

            //We ignore quests without sections/tasks
            quests.Set(parsedQuests.Where(x => x.sections != null && x.sections.Length > 0).Select(x => (x.id, x)));
        }

        /// <summary>
        /// Update progress in a quest
        /// </summary>
        /// <param name="progressedQuest"></param>
        public void UpdateQuestProgress(QuestModel progressedQuest)
        {
            if (!progressedQuest.canBePinned)
                pinnedQuests.Remove(progressedQuest.id);

            //Alex: Edge case. Quests has no sections/tasks, we ignore the UpdateQuestProgress and remove the cached one.
            if (progressedQuest.sections == null || progressedQuest.sections.Length == 0)
            {
                quests.Remove(progressedQuest.id);
                return;
            }

            //Alex: Edge case. Progressed quest was not included in the initialization. We dont invoke quests events
            if (!quests.TryGetValue(progressedQuest.id, out QuestModel oldQuest))
            {
                quests.Add(progressedQuest.id, progressedQuest);
                return;
            }

            List<Action> eventsQueue = new List<Action>();
            quests[progressedQuest.id] = progressedQuest;

            //Events are deferred until everything is processed and the "justProgressed" flags are set
            eventsQueue.Add(() => OnQuestProgressed?.Invoke(progressedQuest.id));

            for (int index = 0; index < progressedQuest.sections.Length; index++)
            {
                QuestSection newQuestSection = progressedQuest.sections[index];
                QuestSection nextQuestSection = (index + 1) < progressedQuest.sections.Length ? (progressedQuest.sections[index + 1]) : null;

                //Alex: Edge case. New quest reported contains a section that was previously not contained.
                // if it's completed, we call the SectionCompleted event and unlock the next one
                bool oldQuestSectionFound = oldQuest.TryGetSection(newQuestSection.id, out QuestSection oldQuestSection);
                bool sectionCompleted = (!oldQuestSectionFound && newQuestSection.progress >= 1) || (Math.Abs(oldQuestSection.progress - newQuestSection.progress) > Mathf.Epsilon && newQuestSection.progress >= 1);

                for (int index2 = 0; index2 < newQuestSection.tasks.Length; index2++)
                {
                    QuestTask currentTask = newQuestSection.tasks[index2];
                    if (oldQuestSectionFound)
                    {
                        currentTask.justProgressed = !oldQuestSection.TryGetTask(currentTask.id, out QuestTask oldTask) || currentTask.progress != oldTask.progress;
                    }
                    else
                    {
                        currentTask.justProgressed = false;
                    }
                }

                if (sectionCompleted)
                {
                    eventsQueue.Add(() => OnSectionCompleted?.Invoke(progressedQuest.id, newQuestSection.id));
                    if (nextQuestSection != null)
                        eventsQueue.Add(() => OnSectionUnlocked?.Invoke(progressedQuest.id, nextQuestSection.id));
                }
            }

            if (!oldQuest.isCompleted && progressedQuest.isCompleted)
                eventsQueue.Add(() => OnQuestCompleted?.Invoke(progressedQuest.id));

            for (int index = 0; index < progressedQuest.rewards.Length; index++)
            {
                QuestReward newReward = progressedQuest.rewards[index];

                //Alex: Edge case. New quest reported contains a reward that was previously not contained.
                // If it's completed, we call the RewardObtained event
                bool oldRewardFound = oldQuest.TryGetReward(newReward.id, out QuestReward oldReward);
                bool rewardObtained = (!oldRewardFound && newReward.status == QuestsLiterals.RewardStatus.ALREADY_GIVEN) || ( newReward.status != oldReward.status && newReward.status == QuestsLiterals.RewardStatus.ALREADY_GIVEN);
                if (rewardObtained)
                {
                    eventsQueue.Add(() => OnRewardObtained?.Invoke(progressedQuest.id, newReward.id));
                }
            }

            for (int index = 0; index < eventsQueue.Count; index++)
            {
                eventsQueue[index].Invoke();
            }

            //Restore "justProgressed" flags
            for (int index = 0; index < progressedQuest.sections.Length; index++)
            {
                QuestSection section = progressedQuest.sections[index];
                for (var index2 = 0; index2 < section.tasks.Length; index2++)
                {
                    section.tasks[index2].justProgressed = false;
                }
            }
        }

        public void RemoveQuest(QuestModel quest) { quests.Remove(quest.id); }

        private void OnPinnedQuestUpdated(string questId) { PlayerPrefs.SetString(PINNED_QUESTS_KEY, JsonConvert.SerializeObject(pinnedQuests.Get())); }

        public void Dispose()
        {
            pinnedQuests.OnAdded -= OnPinnedQuestUpdated;
            pinnedQuests.OnRemoved -= OnPinnedQuestUpdated;
        }
    }
}