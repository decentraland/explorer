using System;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// This controller is in charge of manage all the base logic related to a SLIDER CONTROL.
    /// </summary>
    public class SliderSettingsControlController : SettingsControlController
    {
        public event Action<string> OnOverrideIndicatorLabel;

        protected void RaiseOnOverrideIndicatorLabel(string newIndicatorLabel)
        {
            OnOverrideIndicatorLabel?.Invoke(newIndicatorLabel);
        }
    }
}