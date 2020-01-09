using UnityEngine;
using System;

namespace DCL
{
    public class Settings : DCL.Singleton<Settings>
    {
        const string QUALITY_SETTINGS_KEY = "Settings.Quality";

        public event Action<GameSettings.QualitySettings> OnQualitySettingsChanged;

        public GameSettings.QualitySettings qualitySettings { get { return currentQualitySettings; } }

        private static GameSettings.QualitySettingsData qualitySettingsPreset = null;

        private GameSettings.QualitySettings currentQualitySettings;

        public Settings()
        {
            if (qualitySettingsPreset == null)
            {
                qualitySettingsPreset = Resources.Load<GameSettings.QualitySettingsData>("ScriptableObjects/QualitySettingsData");
            }

            bool isQualitySettingsSet = false;
            if (PlayerPrefs.HasKey(QUALITY_SETTINGS_KEY))
            {
                try
                {
                    currentQualitySettings = JsonUtility.FromJson<GameSettings.QualitySettings>(PlayerPrefs.GetString(QUALITY_SETTINGS_KEY));
                    isQualitySettingsSet = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            if (!isQualitySettingsSet)
            {
                currentQualitySettings = qualitySettingsPreset[qualitySettingsPreset.Length - 1];
            }
        }

        public void ApplyQualitySettingsPreset(int index)
        {
            if (index >= 0 && index < qualitySettingsPreset.Length)
            {
                ApplyQualitySettings(qualitySettingsPreset[index]);
            }
        }

        public void ApplyQualitySettings(GameSettings.QualitySettings settings)
        {
            currentQualitySettings = settings;
            if (OnQualitySettingsChanged != null) OnQualitySettingsChanged(settings);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetString(QUALITY_SETTINGS_KEY, JsonUtility.ToJson(currentQualitySettings));
            PlayerPrefs.Save();
        }
    }
}
