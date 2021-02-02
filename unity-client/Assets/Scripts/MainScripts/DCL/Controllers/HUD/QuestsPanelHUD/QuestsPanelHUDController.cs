using DCL.Helpers;
using DCL.QuestsController;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDController : IHUD
    {
        private QuestsPanelHUDView view;
        private IQuestsController questsController;
        private static BaseDictionary<string, QuestModel> quests => DataStore.Quests.quests;

        InputAction_Trigger toggleQuestsPanel;

        public void Initialize(IQuestsController newQuestsController)
        {
            questsController = newQuestsController;
            view = QuestsPanelHUDView.Create();

            toggleQuestsPanel = Resources.Load<InputAction_Trigger>("ToggleQuestsPanelHud");
            toggleQuestsPanel.OnTriggered += OnToggleActionTriggered;

            questsController.OnQuestProgressed += OnQuestProgressed;
            quests.OnAdded += OnQuestAdded;
            quests.OnRemoved += OnQuestRemoved;
            quests.OnSet += OnQuestSet;
            OnQuestSet(quests.Get());
        }
        private void OnToggleActionTriggered(DCLAction_Trigger action)
        {
            SetVisibility((!view?.gameObject.activeSelf) ?? false);
        }

        private void OnQuestProgressed(string questId)
        {
            view.AddOrUpdateQuest(questId);
        }

        private void OnQuestAdded(string questId, QuestModel questModel)
        {
            view.AddOrUpdateQuest(questId);
        }

        private void OnQuestRemoved(string questId, QuestModel questModel)
        {
            view.RemoveQuest(questId);
        }

        private void OnQuestSet(IEnumerable<KeyValuePair<string, QuestModel>> quests)
        {
            view.ClearQuests();
            foreach ((string key, QuestModel value) in quests)
            {
                OnQuestAdded(key, value);
            }
        }

        public void SetVisibility(bool visible)
        {
            view?.gameObject.SetActive(visible);
        }

        public void Dispose()
        {
            toggleQuestsPanel.OnTriggered -= OnToggleActionTriggered;
            questsController.OnQuestProgressed -= OnQuestProgressed;
            quests.OnAdded -= OnQuestAdded;
            quests.OnRemoved -= OnQuestRemoved;
            quests.OnSet -= OnQuestSet;
        }
    }
}