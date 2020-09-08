namespace DCL.SettingsHUD
{
    public class SettingsHUDController : IHUD
    {
        public SettingsHUDView view { private set; get; }

        public event System.Action OnClose;

        public SettingsHUDController()
        {
            view = SettingsHUDView.Create();
        }

        public void SetVisibility(bool visible)
        {
            if (!visible && view.isOpen)
                OnClose?.Invoke();

            view.SetVisibility(visible);
        }

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }
    }
}