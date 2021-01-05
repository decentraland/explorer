using System.Collections;
using DCL.Controllers;
using DCL.Helpers;

namespace DCL.Components
{
    public class DCLAudioStream : BaseComponent, IOutOfSceneBoundariesHandler
    {
        [System.Serializable]
        public class Model
        {
            public string url;
            public bool playing = false;
            public float volume = 1;
        }

        public Model model;
        private bool isPlaying = false;
        private float settingsVolume = 0;
        private bool isDestroyed = false;

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if(isDestroyed)
                yield break;

            Model prevModel = model;
            model = Utils.SafeFromJson<Model>(newJson);

            bool forceUpdate = prevModel.volume != model.volume;
            settingsVolume = Settings.i.generalSettings.sfxVolume;

            UpdatePlayingState(forceUpdate);

            yield return null;
        }

        private void Start()
        {
            CommonScriptableObjects.sceneID.OnChange += OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
            Settings.i.OnGeneralSettingsChanged += OnSettingsChanged;
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            CommonScriptableObjects.sceneID.OnChange -= OnSceneChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            Settings.i.OnGeneralSettingsChanged -= OnSettingsChanged;
            StopStreaming();
        }

        private bool IsPlayerInSameSceneAsComponent(string currentSceneId)
        {
            if (scene == null) return false;
            if (string.IsNullOrEmpty(currentSceneId)) return false;
            return scene.sceneData.id == currentSceneId;
        }

        private void UpdatePlayingState(bool forceStateUpdate)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            bool canPlayStream = IsPlayerInSameSceneAsComponent(CommonScriptableObjects.sceneID) && CommonScriptableObjects.rendererState;

            bool shouldStopStream = (isPlaying && !model.playing) || (isPlaying && !canPlayStream);
            bool shouldStartStream = !isPlaying && canPlayStream && model.playing;

            if (shouldStopStream)
            {
                StopStreaming();
                return;
            }

            if (shouldStartStream)
            {
                StartStreaming();
                return;
            }

            if (forceStateUpdate)
            {
                if (isPlaying) StartStreaming();
                else StopStreaming();
            }
        }

        private void OnSceneChanged(string sceneId, string prevSceneId)
        {
            UpdatePlayingState(false);
        }

        private void OnRendererStateChanged(bool isEnable, bool prevState)
        {
            if (isEnable)
            {
                UpdatePlayingState(false);
            }
        }

        private void OnSettingsChanged(SettingsData.GeneralSettings settings)
        {
            if (settingsVolume != settings.sfxVolume)
            {
                settingsVolume = settings.sfxVolume;
                UpdatePlayingState(true);
            }
        }

        private void StopStreaming()
        {
            isPlaying = false;
            Interface.WebInterface.SendAudioStreamEvent(model.url, false, model.volume * settingsVolume);
        }

        private void StartStreaming()
        {
            isPlaying = true;
            Interface.WebInterface.SendAudioStreamEvent(model.url, true, model.volume * settingsVolume);
        }

        public void UpdateOutOfBoundariesState(bool enable)
        {
            if (!isPlaying)
                return;

            if (enable)
            {
                StartStreaming();
            }
            else
            {
                //Set volume to 0 (temporary solution until the refactor in #1421)
                Interface.WebInterface.SendAudioStreamEvent(model.url, true, 0);
            }
        }
    }
}