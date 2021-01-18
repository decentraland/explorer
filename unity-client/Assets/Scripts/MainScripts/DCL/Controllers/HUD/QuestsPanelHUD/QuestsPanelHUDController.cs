namespace DCL.Huds.QuestsPanel
{
    public class QuestsPanelHUDController : IHUD
    {
        private QuestsPanelHUDView view;

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            view = QuestsPanelHUDView.Create();
        }

        public void Populate(QuestModel[] quests)
        {
            view.Populate(quests);
        }

        public void SetVisibility(bool visible)
        {
            view?.gameObject.SetActive(visible);
        }
    }
}