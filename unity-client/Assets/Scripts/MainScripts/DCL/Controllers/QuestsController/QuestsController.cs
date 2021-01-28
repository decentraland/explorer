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
    public delegate void SectionCompleted(string questId, string sectionId);
    public delegate void SectionUnlocked(string questId, string sectionId);
    public delegate void TaskProgressed(string questId, string sectionId, string taskId);

    public interface IQuestsController
    {
        event QuestProgressed OnQuestProgressed;
        event SectionCompleted OnSectionCompleted;
        event SectionUnlocked OnSectionUnlocked;
        event TaskProgressed OnTaskProgressed;
    }

    public class QuestsController : MonoBehaviour, IQuestsController
    {
        private const string PINNED_QUESTS_KEY = "PinnedQuests";

        public event QuestProgressed OnQuestProgressed;
        public event SectionCompleted OnSectionCompleted;
        public event SectionUnlocked OnSectionUnlocked;
        public event TaskProgressed OnTaskProgressed;

        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;

        private void Awake()
        {
            var savedPinnedQuests = PlayerPrefs.GetString(PINNED_QUESTS_KEY, null);
            if (!string.IsNullOrEmpty(savedPinnedQuests))
            {
                pinnedQuests.Set(Utils.ParseJsonArray<string[]>(savedPinnedQuests));
            }
            pinnedQuests.OnAdded += SavePinnedQuests;
            pinnedQuests.OnRemoved += SavePinnedQuests;
        }
        private void SavePinnedQuests(string s)
        {
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
            var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
            DataStore.Quests.quests.Set(quests.Select(x => (x.id, x)));
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
        #endregion
    }
}