using System;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// This controller is in charge of manage all the base logic related to a SPIN-BOX CONTROL.
    /// </summary>
    public class SpinBoxSettingsControlController : SettingsControlController
    {
        public event Action<string[]> OnSetLabels;
        protected void RaiseOnOverrideIndicatorLabel(string[] labels)
        {
            OnSetLabels?.Invoke(labels);
        }

        public event Action<string> OnOverrideCurrentLabel;
        protected void RaiseOnOverrideCurrentLabel(string newCurrentLabel)
        {
            OnOverrideCurrentLabel?.Invoke(newCurrentLabel);
        }
    }
}