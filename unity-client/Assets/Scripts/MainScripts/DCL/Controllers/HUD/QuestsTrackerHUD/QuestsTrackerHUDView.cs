using DCL.Helpers;
using System;
using UnityEngine;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDView : MonoBehaviour
    {
        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;

        private bool layoutRebuildRequested;

        public static QuestsTrackerHUDView Create()
        {
            throw new NotImplementedException();
        }

        public void Populate(QuestModel[] quests)
        {
            foreach (var quest in quests)
            {
                CreateQuestEntry(quest);
            }
        }

        internal void CreateQuestEntry(QuestModel quest)
        {
            var taskUIEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestsTrackerEntry>();
            taskUIEntry.OnLayoutRebuildRequested += () => layoutRebuildRequested = true;
            taskUIEntry.Populate(quest);
        }

        private void Update()
        {
            if (layoutRebuildRequested)
            {
                layoutRebuildRequested = false;
                Utils.ForceRebuildLayoutImmediate(questsContainer);
            }
        }
    }
}