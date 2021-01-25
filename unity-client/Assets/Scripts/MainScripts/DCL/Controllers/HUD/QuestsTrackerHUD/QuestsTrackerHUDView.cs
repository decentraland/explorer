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
        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;

        private readonly Dictionary<string, QuestsTrackerEntry> currentEntries = new Dictionary<string, QuestsTrackerEntry>();
        private readonly Dictionary<string, DateTime> lastUpdateTimestamp = new Dictionary<string, DateTime>();
        private bool layoutRebuildRequested;

        public static QuestsTrackerHUDView Create()
        {
            return Instantiate(Resources.Load<GameObject>("QuestsTrackerHUD")).GetComponent<QuestsTrackerHUDView>();
        }

        private void Awake()
        {
            StartCoroutine(DispatchEntriesRoutine());
        }

        public void AddQuest(string questId, bool isPinned)
        {
            if (!DataStore.Quests.quests.TryGetValue(questId, out QuestModel quest))
            {
                Debug.LogError($"Couldn't find quest with ID {questId} in DataStore");
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
            if (currentEntries.ContainsKey(questId))
                RefreshLastUpdateTime(questId, true);
            else
                AddQuest(questId, true);
        }

        public void UnpinQuest(string questId)
        {
            if (!currentEntries.ContainsKey(questId))
                return;

            RefreshLastUpdateTime(questId, false);
        }

        private void Update()
        {
            if (layoutRebuildRequested)
            {
                layoutRebuildRequested = false;
                Utils.ForceRebuildLayoutImmediate(questsContainer);
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

        private IEnumerator DispatchEntriesRoutine()
        {
            while (true)
            {
                var entriesToRemove = lastUpdateTimestamp.Where(x => (DateTime.Now - x.Value) > TimeSpan.FromSeconds(3)).Select(x => x.Key).ToArray();
                foreach (string questId in entriesToRemove)
                {
                    var entry = currentEntries[questId];
                    currentEntries.Remove(questId);
                    lastUpdateTimestamp.Remove(questId);
                    Destroy(entry.gameObject);
                }
                yield return WaitForSecondsCache.Get(0.25f);
            }
        }
    }
}