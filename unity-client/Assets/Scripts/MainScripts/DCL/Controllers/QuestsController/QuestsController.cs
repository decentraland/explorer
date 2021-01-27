using DCL.Helpers;
using System;
using System.Collections.Generic;
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
        public event QuestProgressed OnQuestProgressed;
        public event SectionCompleted OnSectionCompleted;
        public event SectionUnlocked OnSectionUnlocked;
        public event TaskProgressed OnTaskProgressed;

        #region just for testing, dont merge this code. (IF you see this in a review, hit me)
        [SerializeField] private TextAsset questsJson;
        private void Start()
        {
            var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
            for (var i = quests.Count - 1; i >= 0; i--)
            {
                DataStore.Quests.quests.Add(quests[i].id, quests[i]);
            }
        }
        [ContextMenu("Add Entry")]
        private void UtilAddEntry()
        {
            var quests = Utils.ParseJsonArray<List<QuestModel>>(questsJson.text);
            OnQuestProgressed?.Invoke(quests[Random.Range(0, quests.Count)].id);
        }
        #endregion
    }
}