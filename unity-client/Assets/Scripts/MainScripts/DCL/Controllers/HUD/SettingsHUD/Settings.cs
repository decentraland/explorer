using UnityEngine;
using System;

namespace DCL
{
    public class Settings : DCL.Singleton<Settings>
    {
        const string QUALITY_SETTINGS_KEY = "Settings.Quality";
        const string GENERAL_SETTINGS_KEY = "Settings.General";

        public event Action<GameSettings.QualitySettings> OnQualitySettingsChanged;
        public event Action<GameSettings.GeneralSettings> OnGeneralSettingsChanged;

        public GameSettings.QualitySettings qualitySettings { get { return currentQualitySettings; } }
        public GameSettings.QualitySettingsData qualitySettingsPresets { get { return qualitySettingsPreset; } }
        public GameSettings.GeneralSettings generalSettings { get { return currentGeneralSettings; } }

        private static GameSettings.QualitySettingsData qualitySettingsPreset = null;

        private GameSettings.QualitySettings currentQualitySettings;
        private GameSettings.GeneralSettings currentGeneralSettings;

        public Settings()
        {
            if (qualitySettingsPreset == null)
            {
                qualitySettingsPreset = Resources.Load<GameSettings.QualitySettingsData>("ScriptableObjects/QualitySettingsData");
            }
            LoadQualitySettings();
            LoadGeneralSettings();
        }

        private void LoadQualitySettings()
        {
            bool isQualitySettingsSet = false;
            if (PlayerPrefs.HasKey(QUALITY_SETTINGS_KEY))
            {
                try
                {
                    currentQualitySettings = JsonUtility.FromJson<GameSettings.QualitySettings>(PlayerPrefs.GetString(QUALITY_SETTINGS_KEY));
                    Debug.Log($"currentQualitySettings: {PlayerPrefs.GetString(QUALITY_SETTINGS_KEY)}");
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

        private void LoadGeneralSettings()
        {
            bool isGeneralSettingsSet = false;
            if (PlayerPrefs.HasKey(GENERAL_SETTINGS_KEY))
            {
                try
                {
                    currentGeneralSettings = JsonUtility.FromJson<GameSettings.GeneralSettings>(PlayerPrefs.GetString(GENERAL_SETTINGS_KEY));
                    isGeneralSettingsSet = true;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            if (!isGeneralSettingsSet)
            {
                currentGeneralSettings = new GameSettings.GeneralSettings()
                {
                    sfxVolume = 1,
                    mouseSensitivity = 1
                };
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

        public void ApplyGeneralSettings(GameSettings.GeneralSettings settings)
        {
            currentGeneralSettings = settings;
            if (OnGeneralSettingsChanged != null) OnGeneralSettingsChanged(settings);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetString(GENERAL_SETTINGS_KEY, JsonUtility.ToJson(currentGeneralSettings));
            PlayerPrefs.SetString(QUALITY_SETTINGS_KEY, JsonUtility.ToJson(currentQualitySettings));
            PlayerPrefs.Save();
        }
    }
}

namespace DCL.GameSettings
{
    [Serializable]
    public struct GeneralSettings
    {
        public float sfxVolume;
        public float mouseSensitivity;
    }
}