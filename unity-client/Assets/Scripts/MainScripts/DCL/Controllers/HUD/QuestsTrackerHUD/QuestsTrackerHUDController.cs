namespace DCL.Huds.QuestsTracker
{
    public class QuestsTrackerHUDController : IHUD
    {
        private QuestsTrackerHUDView view;

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            view = QuestsTrackerHUDView.Create();
        }

        public void SetVisibility(bool visible)
        {
            view?.gameObject.SetActive(visible);
        }
    }
}
