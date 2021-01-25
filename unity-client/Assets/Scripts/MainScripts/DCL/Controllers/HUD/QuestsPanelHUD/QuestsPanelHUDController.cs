﻿using DCL.Helpers;
using System.Collections.Generic;

namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDController : IHUD
    {
        private QuestsPanelHUDView view;
        private IQuestsController questsController;

        public void Initialize(IQuestsController newQuestsController)
        {
            questsController = newQuestsController;
            view = QuestsPanelHUDView.Create();

            questsController.OnQuestProgressed += OnQuestProgressed;
            DataStore.Quests.quests.OnAdded += OnQuestAdded;
            DataStore.Quests.quests.OnRemoved += OnQuestRemoved;
            DataStore.Quests.quests.OnSet += OnQuestSet;
            OnQuestSet(DataStore.Quests.quests.Get());
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
            questsController.OnQuestProgressed -= OnQuestProgressed;
            DataStore.Quests.quests.OnAdded -= OnQuestAdded;
            DataStore.Quests.quests.OnRemoved -= OnQuestRemoved;
            DataStore.Quests.quests.OnSet -= OnQuestSet;
        }
    }
}