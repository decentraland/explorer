using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDView : MonoBehaviour
    {
        private const int ENTRIES_PER_FRAME = 5;
        private const string VIEW_PATH = "QuestsPanelHUD";

        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;
        [SerializeField] private QuestsPanelPopup questPopup;

        private static BaseDictionary<string, QuestModel> quests => DataStore.Quests.quests;

        private string currentQuestInPopup = "";
        private readonly Dictionary<string, QuestsPanelEntry> questEntries =  new Dictionary<string, QuestsPanelEntry>();
        private bool layoutRebuildRequested = false;
        private readonly List<string> questsToBeAdded = new List<string>();

        internal static QuestsPanelHUDView Create()
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuestsPanelHUDView>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestsPanelHUDView";
#endif
            return view;
        }

        public void Awake()
        {
            questPopup.gameObject.SetActive(false);
        }

        public void RequestAddOrUpdateQuest(string questId)
        {
            if (questsToBeAdded.Contains(questId))
                return;

            questsToBeAdded.Add(questId);
        }

        private void AddOrUpdateQuest(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
            {
                Debug.LogError($"Couldn't find quest with ID {questId} in DataStore");
                return;
            }

            if(!questEntries.TryGetValue(questId, out QuestsPanelEntry questEntry))
            {
                questEntry = Instantiate(questPrefab, questsContainer).GetComponent<QuestsPanelEntry>();
                questEntry.OnReadMoreClicked += ShowQuestPopup;
                questEntries.Add(questId, questEntry);
            }

            questEntry.Populate(quest);
            layoutRebuildRequested = true;
        }

        public void RemoveQuest(string questId)
        {
            questsToBeAdded.Remove(questId);

            if(!questEntries.TryGetValue(questId, out QuestsPanelEntry questEntry))
                return;

            questEntries.Remove(questId);
            Destroy(questEntry.gameObject);

            if(currentQuestInPopup == questId)
                questPopup.Close();
        }

        public void ClearQuests()
        {
            questPopup.Close();
            foreach (QuestsPanelEntry questEntry in questEntries.Values)
            {
                Destroy(questEntry.gameObject);
            }
            questEntries.Clear();
        }

        internal void ShowQuestPopup(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
            {
                Debug.Log($"Couldnt find quest with id {questId}");
                return;
            }

            currentQuestInPopup = questId;
            questPopup.Populate(quest);
            questPopup.Show();
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
                AddOrUpdateQuest(questId);
            }
        }
    }
}