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

            if (!string.IsNullOrEmpty(model.url) && (previousModel == null || previousModel.url != model.url))
            {
                player.url = model.url;
                player.Play();
            }
            if (string.IsNullOrEmpty(model.url) && previousModel != null)
            {
                player.Stop();
                player.url = null;
            }
            yield return null;
        }
    }
}
