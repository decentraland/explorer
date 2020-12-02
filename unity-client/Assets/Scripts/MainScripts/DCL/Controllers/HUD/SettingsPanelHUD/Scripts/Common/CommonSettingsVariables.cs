using UnityEngine;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsVariables
    {
        private static BooleanVariable shouldSetQualityPresetAsCustomValue = new BooleanVariable();
        public static BooleanVariable shouldSetQualityPresetAsCustom => GetOrCreate(ref shouldSetQualityPresetAsCustomValue);

        internal static T GetOrCreate<T>(ref T variable) where T : Object
        {
            if (variable == null)
            {
                variable = new BaseVariable<T>();
            }

            return variable;
        }
    }
}