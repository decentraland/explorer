using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public class DCLAudioStream : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public string url;
            public bool playing = false;
        }

        public Model model;
        private bool isPlaying = false;

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.url))
            {
                UpdatePlayingState();
            }

            yield return null;
        }

        private void Start()
        {
            CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChanged;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChanged;
        }

        private void OnDestroy()
        {
            CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChanged;
            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChanged;
            StopStreaming();
        }

        private bool AreCoordsInsideComponentScene(Vector2Int coords)
        {
            return scene.parcels.Contains(coords);
        }

        private void UpdatePlayingState()
        {
            if (gameObject.activeInHierarchy)
            {
                bool canPlay = AreCoordsInsideComponentScene(CommonScriptableObjects.playerCoords.Get()) && CommonScriptableObjects.rendererState.Get();
                bool shouldBePlaying = model.playing;

                if (isPlaying && !shouldBePlaying)
                {
                    StopStreaming();
                }
                else if (isPlaying && !canPlay)
                {
                    StopStreaming();
                }
                else if (!isPlaying && canPlay && shouldBePlaying)
                {
                    StartStreaming();
                }
            }
        }

        private void OnPlayerCoordsChanged(Vector2Int coords, Vector2Int prevCoords)
        {
            UpdatePlayingState();
        }

        private void OnRendererStateChanged(bool isEnable, bool prevState)
        {
            if (isEnable)
            {
                UpdatePlayingState();
            }
        }

        private void StopStreaming()
        {
            isPlaying = false;
            Interface.WebInterface.SendAudioStreamEvent(model.url, false);
        }

        private void StartStreaming()
        {
            isPlaying = true;
            Interface.WebInterface.SendAudioStreamEvent(model.url, true);
        }
    }
}
