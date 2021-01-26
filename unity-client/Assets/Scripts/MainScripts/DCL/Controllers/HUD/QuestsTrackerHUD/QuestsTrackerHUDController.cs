using DCL.QuestsController;

namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDController : IHUD
    {
        private QuestsTrackerHUDView view;
        private IQuestsController questsController;


        public void Initialize(IQuestsController controller)
        {
            questsController = controller;
            view = QuestsTrackerHUDView.Create();

            questsController.OnQuestProgressed += OnQuestProgressed;
            DataStore.Quests.pinnedQuests.OnAdded += OnPinnedQuests;
            DataStore.Quests.pinnedQuests.OnRemoved += OnUnpinnedQuests;
        }

        private void OnQuestProgressed(string questId)
        {
            view?.AddQuest(questId, DataStore.Quests.pinnedQuests.Contains(questId));
        }

        private void OnPinnedQuests(string questId)
        {
            view?.PinQuest(questId);
        }

        private void OnUnpinnedQuests(string questId)
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
            DataStore.Quests.pinnedQuests.OnAdded -= OnPinnedQuests;
            DataStore.Quests.pinnedQuests.OnRemoved -= OnUnpinnedQuests;
        }
    }
}