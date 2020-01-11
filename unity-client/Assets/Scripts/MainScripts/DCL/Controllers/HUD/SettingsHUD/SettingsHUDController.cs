namespace DCL.SettingsHUD
{
    public class SettingsHUDController : IHUD
    {
        private SettingsHUDView view;

        public SettingsHUDController()
        {
            view = SettingsHUDView.Create();
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }
    }
}
