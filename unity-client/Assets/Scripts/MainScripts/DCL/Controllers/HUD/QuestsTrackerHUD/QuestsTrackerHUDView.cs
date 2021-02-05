using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDView : MonoBehaviour
    {
        private const int ENTRIES_PER_FRAME = 5;

        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;

        private readonly Dictionary<string, QuestsTrackerEntry> currentEntries = new Dictionary<string, QuestsTrackerEntry>();
        private readonly Dictionary<string, DateTime> lastUpdateTimestamp = new Dictionary<string, DateTime>();
        private bool layoutRebuildRequested;
        private static BaseDictionary<string, QuestModel> quests => DataStore.Quests.quests;
        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;
        private readonly List<string> questsToBeAdded = new List<string>();

        public static QuestsTrackerHUDView Create()
        {
            QuestsTrackerHUDView view = Instantiate(Resources.Load<GameObject>("QuestsTrackerHUD")).GetComponent<QuestsTrackerHUDView>();

#if UNITY_EDITOR
            view.gameObject.name = "_QuestsTrackerHUDView";
#endif
            return view;
        }

        private void Awake()
        {
            StartCoroutine(DispatchEntriesRoutine());
        }

        public void RequestAddOrUpdateQuest(string questId)
        {
            if (questsToBeAdded.Contains(questId))
                return;

            questsToBeAdded.Add(questId);
        }

        private void AddOrUpdateQuest(string questId, bool isPinned)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) )
                return;

            if (quest.isCompleted)
            {
                RemoveEntry(questId);
                return;
            }

            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry questEntry))
            {
                questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestsTrackerEntry>();
                questEntry.OnLayoutRebuildRequested += () => layoutRebuildRequested = true;
                questEntry.Populate(quest);
                currentEntries.Add(quest.id, questEntry);
            }

            RefreshLastUpdateTime(quest.id, isPinned);
            questEntry.transform.SetSiblingIndex(0);

            questEntry.Populate(quest);
            questEntry.SetPinStatus(isPinned);
            layoutRebuildRequested = true;
        }

        public void PinQuest(string questId)
        {
            if (currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
            {
                entry.SetPinStatus(true);
                RefreshLastUpdateTime(questId, true);
            }
            else
                AddOrUpdateQuest(questId, true);
        }

        public void UnpinQuest(string questId)
        {
            if (!currentEntries.TryGetValue(questId, out QuestsTrackerEntry entry))
                return;

            entry.SetPinStatus(false);
            RefreshLastUpdateTime(questId, false);
        }

        private void Update()
        {
            if (layoutRebuildRequested)
            {
                layoutRebuildRequested = false;
                Utils.ForceRebuildLayoutImmediate(questsContainer);
            }

            for (int i = 0; i < ENTRIES_PER_FRAME && questsToBeAdded.Count > 0; i++)
            {
                string questId = questsToBeAdded.First();
                questsToBeAdded.RemoveAt(0);
                AddOrUpdateQuest(questId, pinnedQuests.Contains(questId));
            }
        }

        private void RefreshLastUpdateTime(string questId, bool isPinned)
        {
            DateTime dateToSet = isPinned ? DateTime.MaxValue : DateTime.Now;

            if (lastUpdateTimestamp.ContainsKey(questId))
                lastUpdateTimestamp[questId] = dateToSet;
            else
                lastUpdateTimestamp.Add(questId, dateToSet);
        }

        public void ClearEntries()
        {
            lastUpdateTimestamp.Clear();
            foreach ((string key, QuestsTrackerEntry value) in currentEntries)
            {
                Destroy(value.gameObject);
            }
            currentEntries.Clear();
        }

        private IEnumerator DispatchEntriesRoutine()
        {
            while (true)
            {
                var entriesToRemove = lastUpdateTimestamp.Where(x => (DateTime.Now - x.Value) > TimeSpan.FromSeconds(3)).Select(x => x.Key).ToArray();
                foreach (string questId in entriesToRemove)
                {
                    RemoveEntry(questId);
                }
                yield return WaitForSecondsCache.Get(0.25f);
            }
        }

        private void RemoveEntry(string questId)
        {
            var entry = currentEntries[questId];
            currentEntries.Remove(questId);
            lastUpdateTimestamp.Remove(questId);
            Destroy(entry.gameObject);
        }
    }
}