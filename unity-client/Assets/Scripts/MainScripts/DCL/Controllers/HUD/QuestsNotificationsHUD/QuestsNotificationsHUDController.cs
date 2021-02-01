using DCL.QuestsController;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestsNotificationsHUDController : IHUD
    {
        private IQuestsController questsController;

        private QuestsNotificationsHUDView view;
        private static BaseDictionary<string, QuestModel> quests => DataStore.Quests.quests;

        public void Initialize(IQuestsController newQuestsController)
        {
            view = QuestsNotificationsHUDView.Create();

            questsController = newQuestsController;

            questsController.OnSectionCompleted += OnSectionCompleted;
            questsController.OnSectionUnlocked += OnSectionUnlocked;
            questsController.OnQuestCompleted += OnQuestCompleted;
        }

        private void OnQuestCompleted(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
                return;

            view?.ShowQuestCompleted(quest);
        }

        private void OnSectionCompleted(string questId, string sectionId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
                return;

            if (!quest.TryGetSection(sectionId, out QuestSection section))
                return;

            view?.ShowSectionCompleted(section);
        }

        private void OnSectionUnlocked(string questId, string sectionId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest))
                return;

            if (!quest.TryGetSection(sectionId, out QuestSection section))
                return;

            view?.ShowSectionUnlocked(section);
        }

        public void SetVisibility(bool visible)
        {
            view?.gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            questsController.OnSectionCompleted -= OnSectionCompleted;
            questsController.OnSectionUnlocked -= OnSectionUnlocked;
            questsController.OnQuestCompleted -= OnQuestCompleted;
        }
    }

}