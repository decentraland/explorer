using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDView : MonoBehaviour
    {
        private const string VIEW_PATH = "QuestsPanelHUD";

        [SerializeField] private RectTransform questsContainer;
        [SerializeField] private GameObject questPrefab;
        [SerializeField] private QuestsPanelPopup questPopup;

        private string currentQuestInPopup = "";
        private readonly Dictionary<string, QuestsPanelEntry> questEntries =  new Dictionary<string, QuestsPanelEntry>();

        private bool layoutRebuildRequested = false;

        internal static QuestsPanelHUDView Create()
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuestsPanelHUDView>();
#if UNITY_EDITOR
            view.gameObject.name = "_QuestHUDPanelView";
#endif
            return view;
        }

        public void Awake()
        {
            questPopup.gameObject.SetActive(false);
        }

        public void AddOrUpdateQuest(string questId)
        {
            if (!DataStore.Quests.quests.TryGetValue(questId, out QuestModel quest))
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
            if(!questEntries.TryGetValue(questId, out QuestsPanelEntry questEntry))
                return;

            questEntries.Remove(questId);
            Destroy(questEntry.gameObject);

            if(currentQuestInPopup == questId)
                questPopup.Close();
        }

        internal void ShowQuestPopup(string questId)
        {
            if (!DataStore.Quests.quests.TryGetValue(questId, out QuestModel quest))
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
        }
    }
}