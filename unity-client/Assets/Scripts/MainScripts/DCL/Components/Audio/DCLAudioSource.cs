using DCL.Helpers;
using System.Collections;
using DCL.Controllers;
using UnityEngine;
using DCL.Models;
using System.Collections.Generic;

namespace DCL.Components
{
    public class DCLAudioSource : BaseComponent, IOutOfSceneBoundariesHandler
    {
        [System.Serializable]
        public class Model : BaseModel
        {
            public string audioClipId;
            public bool playing;
            public float volume = 1f;
            public bool loop = false;
            public float pitch = 1f;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }

            public override bool Equals(object obj)
            {
                var item = obj as Model;

                if (item == null)
                {
                    return false;
                }

                return audioClipId.Equals(item.audioClipId) ||
                       playing.Equals(item.playing) ||
                       volume.Equals(item.volume) ||
                       loop.Equals(item.loop) ||
                       pitch.Equals(item.pitch);
            }

            public override int GetHashCode()
            {
                int hashCode = 1859268890;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(audioClipId);
                hashCode = hashCode * -1521134295 + playing.GetHashCode();
                hashCode = hashCode * -1521134295 + volume.GetHashCode();
                hashCode = hashCode * -1521134295 + loop.GetHashCode();
                hashCode = hashCode * -1521134295 + pitch.GetHashCode();
                return hashCode;
            }
        }

        public float playTime => audioSource.time;
        internal AudioSource audioSource;
        DCLAudioClip lastDCLAudioClip;

        private bool isDestroyed = false;

        private void Awake()
        {
            audioSource = gameObject.GetOrCreateComponent<AudioSource>();
        }

        public void InitDCLAudioClip(DCLAudioClip dclAudioClip)
        {
            if (lastDCLAudioClip != null)
            {
                lastDCLAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
            }

            lastDCLAudioClip = dclAudioClip;
        }

        public double GetVolume()
        {
            return ((Model)model).volume;
        }

        public override IEnumerator ApplyChanges(BaseModel baseModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());

            //If the scene creates and destroy an audiosource before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDestroyed)
                yield break;

            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;
            CommonScriptableObjects.sceneID.OnChange += OnCurrentSceneChanged;

            ApplyCurrentModel();

            yield return null;
        }

        private void ApplyCurrentModel()
        {
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource is null!.");
                return;
            }

            Model model = (Model) this.model;
            audioSource.volume = (scene.sceneData.id == CommonScriptableObjects.sceneID.Get()) ? model.volume : 0f;
            audioSource.loop = model.loop;
            audioSource.pitch = model.pitch;
            audioSource.spatialBlend = 1;
            audioSource.dopplerLevel = 0.1f;

            if (model.playing)
            {
                DCLAudioClip dclAudioClip = scene.GetSharedComponent(model.audioClipId) as DCLAudioClip;

                if (dclAudioClip != null)
                {
                    InitDCLAudioClip(dclAudioClip);
                    //NOTE(Brian): Play if finished loading, otherwise will wait for the loading to complete (or fail).
                    if (dclAudioClip.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED)
                    {
                        ApplyLoadedAudioClip(dclAudioClip);
                    }
                    else
                    {
                        dclAudioClip.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
                        dclAudioClip.OnLoadingFinished += DclAudioClip_OnLoadingFinished;
                    }
                }
                else
                {
                    Debug.LogError("Wrong audio clip type when playing audiosource!!");
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }

        private void OnCurrentSceneChanged(string currentSceneId, string previousSceneId)
        {
            if (audioSource != null)
            {
                Model model = (Model)this.model;
                audioSource.volume = (scene.sceneData.id == currentSceneId) ? model.volume : 0f;
            }
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            CommonScriptableObjects.sceneID.OnChange -= OnCurrentSceneChanged;

            //NOTE(Brian): Unsuscribe events.
            InitDCLAudioClip(null);
        }

        public void UpdateOutOfBoundariesState(bool isEnabled)
        {
            bool isDirty = audioSource.enabled != isEnabled;
            audioSource.enabled = isEnabled;
            if (isDirty && isEnabled)
            {
                ApplyCurrentModel();
            }
        }

        private void DclAudioClip_OnLoadingFinished(DCLAudioClip obj)
        {
            if (obj.loadingState == DCLAudioClip.LoadState.LOADING_COMPLETED && audioSource != null)
            {
                ApplyLoadedAudioClip(obj);
            }

            obj.OnLoadingFinished -= DclAudioClip_OnLoadingFinished;
        }

        private void ApplyLoadedAudioClip(DCLAudioClip clip)
        {
            if (audioSource.clip != clip.audioClip)
            {
                audioSource.clip = clip.audioClip;
            }
            Model model = (Model)this.model;
            if (audioSource.enabled && model.playing && !audioSource.isPlaying)
            {
                //To remove a pesky and quite unlikely warning when the audiosource is out of scenebounds
                audioSource.Play();
            }
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.AUDIO_SOURCE;
        }
    }
}