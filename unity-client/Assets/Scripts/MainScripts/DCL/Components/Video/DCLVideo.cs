using DCL.Components;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace DCL.Components
{
    public class DCLVideo : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public string url;
        }

        public Model model;
        public VideoPlayer player;

        public override IEnumerator ApplyChanges(string newJson)
        {
            var previousModel = model;
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (player == null && !string.IsNullOrEmpty(model.url) && (previousModel == null || previousModel.url != model.url))
            {
                if (previousModel != null && player != null)
                {
                    Destroy(player);
                }
                player = gameObject.AddComponent<VideoPlayer>();

                player.playOnAwake = true;
                player.source = VideoSource.Url;
                player.url = model.url;
                player.renderMode = VideoRenderMode.MaterialOverride;
                player.targetMaterialRenderer = GetComponent<Renderer>();
                player.targetMaterialProperty = "Mesh";
                player.Play();
            }
            yield return null;
        }
    }
}
