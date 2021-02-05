using DCL.QuestsController;
using System.Collections.Generic;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDController : IHUD
    {
        private static BaseDictionary<string, QuestModel> quests =>DataStore.Quests.quests;
        private static BaseCollection<string> pinnedQuests => DataStore.Quests.pinnedQuests;

        private QuestsTrackerHUDView view;
        private IQuestsController questsController;


        public void Initialize(IQuestsController controller)
        {
            questsController = controller;
            view = QuestsTrackerHUDView.Create();

            questsController.OnQuestProgressed += OnQuestProgressed;
            pinnedQuests.OnAdded += OnPinnedQuest;
            pinnedQuests.OnRemoved += OnUnpinnedQuest;
            pinnedQuests.OnSet += OnPinnedQuestsSet;
            quests.OnSet += OnQuestsSet;

            foreach (string questId in pinnedQuests.Get())
            {
                view?.PinQuest(questId);
            }
        }

        private void OnQuestsSet(IEnumerable<KeyValuePair<string, QuestModel>> pairs)
        {
            OnPinnedQuestsSet(pinnedQuests.Get());
        }

        private void OnPinnedQuestsSet(IEnumerable<string> pinnedQuests)
        {
            view?.ClearEntries();
            foreach (string questId in pinnedQuests)
            {
                view?.PinQuest(questId);
            }
        }

        private void OnQuestProgressed(string questId)
        {
            view?.RequestAddOrUpdateQuest(questId);
        }

        private void OnPinnedQuest(string questId)
        {
            view?.PinQuest(questId);
        }

        private void OnUnpinnedQuest(string questId)
        {
            view?.UnpinQuest(questId);
        }

        public void SetVisibility(bool visible)
        {
            view?.gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            questsController.OnQuestProgressed -= OnQuestProgressed;
            pinnedQuests.OnAdded -= OnPinnedQuest;
            pinnedQuests.OnRemoved -= OnUnpinnedQuest;
            pinnedQuests.OnSet -= OnPinnedQuestsSet;
            quests.OnSet -= OnQuestsSet;
        }
    }
}