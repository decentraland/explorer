using UnityEngine;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsVariables
    {
        private static BooleanVariable shouldSetQualityPresetAsCustomValue = new BooleanVariable();
        public static BooleanVariable shouldSetQualityPresetAsCustom => GetOrCreate(ref shouldSetQualityPresetAsCustomValue);

        private static BooleanVariable shadowStateValue = new BooleanVariable();
        public static BooleanVariable shadowState => GetOrCreate(ref shadowStateValue);

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