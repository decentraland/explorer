using DCL.QuestsController;

namespace DCL.Huds.QuestsNotifications
{
    public class QuestsNotificationsHUDController : IHUD
    {
        internal IQuestsController questsController;
        internal IQuestsNotificationsHUDView view;
        private static BaseDictionary<string, QuestModel> quests => DataStore.i.Quests.quests;

        public void Initialize(IQuestsController newQuestsController)
        {
            view = CreateView();

            questsController = newQuestsController;

            questsController.OnSectionCompleted += OnSectionCompleted;
            questsController.OnSectionUnlocked += OnSectionUnlocked;
            questsController.OnQuestCompleted += OnQuestCompleted;
            questsController.OnRewardObtained += OnRewardObtained;
        }

        private void OnQuestCompleted(string questId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestsLiterals.Status.BLOCKED)
                return;

            view?.ShowQuestCompleted(quest);
        }

        private void OnSectionCompleted(string questId, string sectionId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestsLiterals.Status.BLOCKED)
                return;

            if (!quest.TryGetSection(sectionId, out QuestSection section))
                return;

            view?.ShowSectionCompleted(section);
        }

        private void OnSectionUnlocked(string questId, string sectionId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestsLiterals.Status.BLOCKED)
                return;

            if (!quest.TryGetSection(sectionId, out QuestSection section))
                return;

            view?.ShowSectionUnlocked(section);
        }

        private void OnRewardObtained(string questId, string rewardId)
        {
            if (!quests.TryGetValue(questId, out QuestModel quest) || quest.status == QuestsLiterals.Status.BLOCKED)
                return;

            if (!quest.TryGetReward(rewardId, out QuestReward reward))
                return;

            view?.ShowRewardObtained(reward);
        }

        public void SetVisibility(bool visible) { view?.SetVisibility(visible); }

        public void Dispose()
        {
            if (questsController != null)
            {
                view?.Dispose();
                questsController.OnSectionCompleted -= OnSectionCompleted;
                questsController.OnSectionUnlocked -= OnSectionUnlocked;
                questsController.OnQuestCompleted -= OnQuestCompleted;
                questsController.OnRewardObtained -= OnRewardObtained;
            }
        }

        internal virtual IQuestsNotificationsHUDView CreateView() => QuestsNotificationsHUDView.Create();
    }
}